using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MauiApp1.Models;

namespace MauiApp1.Services;

public sealed class XlsxWorkbook
{
    private const int MaximumBytes = 10 * 1024 * 1024;

    public async Task<WorkbookPreview> ReadAsync(Stream source)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int count;
        while ((count = await source.ReadAsync(chunk)) > 0)
        {
            if (buffer.Length + count > MaximumBytes) throw new StudyException("VImportLimit");
            await buffer.WriteAsync(chunk.AsMemory(0, count));
        }
        buffer.Position = 0;
        try { return await Task.Run(() => Parse(buffer)); }
        catch (Exception exception) when (exception is InvalidDataException or XmlException or FormatException or OverflowException or ArgumentException)
        {
            throw new StudyException("VImportInvalid", exception);
        }
    }

    private static WorkbookPreview Parse(Stream source)
    {
        using var archive = new ZipArchive(source, ZipArchiveMode.Read, true);
        if (archive.Entries.Count > 2000 || archive.Entries.Sum(entry => entry.Length) > 32L * 1024 * 1024 ||
            archive.Entries.Any(entry => entry.Length > 8L * 1024 * 1024) ||
            archive.Entries.Select(entry => entry.FullName).Distinct().Count() != archive.Entries.Count)
            throw new StudyException("VImportLimit");
        var workbook = ReadXml(archive, "xl/workbook.xml");
        var sheet = workbook.Descendants().FirstOrDefault(element => element.Name.LocalName == "sheet" &&
            (string?)element.Attribute("state") is not ("hidden" or "veryHidden")) ?? throw new StudyException("VImportInvalid");
        var relationshipId = sheet.Attributes().FirstOrDefault(attribute => attribute.Name.LocalName == "id")?.Value;
        var relationships = ReadXml(archive, "xl/_rels/workbook.xml.rels");
        var relation = relationships.Descendants().FirstOrDefault(element => element.Name.LocalName == "Relationship" && (string?)element.Attribute("Id") == relationshipId)
            ?? throw new StudyException("VImportInvalid");
        var target = (string?)relation.Attribute("Target") ?? throw new StudyException("VImportInvalid");
        if ((string?)relation.Attribute("TargetMode") == "External" || target.Contains(':') || target.Contains('\\')) throw new StudyException("VImportInvalid");
        var sheetUri = new Uri(new Uri("https://workbook.invalid/xl/workbook.xml"), target);
        if (sheetUri.Host != "workbook.invalid") throw new StudyException("VImportInvalid");
        var sheetPath = Uri.UnescapeDataString(sheetUri.AbsolutePath.TrimStart('/'));
        var strings = archive.GetEntry("xl/sharedStrings.xml") is null ? [] : ReadXml(archive, "xl/sharedStrings.xml")
            .Descendants().Where(element => element.Name.LocalName == "si").Select(ReadText).ToList();
        var rows = ReadXml(archive, sheetPath).Descendants().Where(element => element.Name.LocalName == "row").ToList();
        if (rows.Count > 2001) throw new StudyException("VImportLimit");
        if (rows.Count == 0) throw new StudyException("VHeadersInvalid");
        var header = ReadCells(rows[0], strings);
        var first = VocabularyRules.Normalize(header.First);
        var second = VocabularyRules.Normalize(header.Second);
        var reversed = IsEnglish(first) && IsVietnamese(second);
        if (header.Error is not null || !(reversed || (IsVietnamese(first) && IsEnglish(second)))) throw new StudyException("VHeadersInvalid");
        var result = new List<WorkbookRow>();
        for (var index = 1; index < rows.Count; index++)
        {
            var cells = ReadCells(rows[index], strings);
            if (string.IsNullOrWhiteSpace(cells.First) && string.IsNullOrWhiteSpace(cells.Second) && cells.Error is null) continue;
            var vietnamese = reversed ? cells.Second : cells.First;
            var english = reversed ? cells.First : cells.Second;
            var rowNumber = int.TryParse((string?)rows[index].Attribute("r"), out var parsed) ? parsed : index + 1;
            var error = cells.Error ?? (!VocabularyRules.IsValidTerm(vietnamese) || !VocabularyRules.IsValidTerm(english) ? "VTermInvalid" : null);
            result.Add(new WorkbookRow(rowNumber, vietnamese.Trim(), english.Trim(), error));
        }
        if (result.Count == 0) throw new StudyException("VImportEmpty");
        return new WorkbookPreview((string?)sheet.Attribute("name") ?? "Sheet1", result);
    }

    private static bool IsVietnamese(string value) => value is "TIẾNG VIỆT" or "VIETNAMESE" or "VI";
    private static bool IsEnglish(string value) => value is "TIẾNG ANH" or "ENGLISH" or "EN";

    private static (string First, string Second, string? Error) ReadCells(XElement row, IReadOnlyList<string> sharedStrings)
    {
        var first = "";
        var second = "";
        string? error = null;
        var position = 0;
        var seen = new HashSet<string>();
        foreach (var cell in row.Elements().Where(element => element.Name.LocalName == "c"))
        {
            position++;
            var reference = (string?)cell.Attribute("r");
            var column = reference is null ? (position == 1 ? "A" : position == 2 ? "B" : "Other") : new string(reference.TakeWhile(char.IsLetter).ToArray()).ToUpperInvariant();
            if (!seen.Add(column)) error = "VImportInvalid";
            if (cell.Elements().Any(element => element.Name.LocalName == "f")) error = "VFormulaUnsupported";
            var type = (string?)cell.Attribute("t");
            var value = cell.Elements().FirstOrDefault(element => element.Name.LocalName == "v")?.Value ?? "";
            if (type == "s")
            {
                if (!int.TryParse(value, out var stringIndex) || stringIndex < 0 || stringIndex >= sharedStrings.Count) throw new StudyException("VImportInvalid");
                value = sharedStrings[stringIndex];
            }
            else if (type == "inlineStr") value = ReadText(cell);
            else if (type is "e" or "b") error = "VImportInvalid";
            if (column == "A") first = value;
            else if (column == "B") second = value;
            else if (!string.IsNullOrWhiteSpace(value)) error = "VTwoColumns";
        }
        return (first, second, error);
    }

    private static string ReadText(XElement element) => string.Concat(element.Descendants()
        .Where(child => child.Name.LocalName == "t" && !child.Ancestors().Any(parent => parent.Name.LocalName == "rPh")).Select(child => child.Value));

    private static XDocument ReadXml(ZipArchive archive, string path)
    {
        var entry = archive.GetEntry(path) ?? throw new StudyException("VImportInvalid");
        using var stream = entry.Open();
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 8 * 1024 * 1024 });
        return XDocument.Load(reader);
    }

    public void Write(Stream output, IEnumerable<VocabularyCard> cards)
    {
        XNamespace spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace relations = "http://schemas.openxmlformats.org/package/2006/relationships";
        XNamespace documentRelations = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        XNamespace contentTypes = "http://schemas.openxmlformats.org/package/2006/content-types";
        using var archive = new ZipArchive(output, ZipArchiveMode.Create, true);
        void Add(string name, XDocument document)
        {
            using var stream = archive.CreateEntry(name).Open();
            using var writer = new StreamWriter(stream, new UTF8Encoding(false));
            document.Save(writer);
        }
        Add("[Content_Types].xml", new XDocument(new XElement(contentTypes + "Types",
            new XElement(contentTypes + "Default", new XAttribute("Extension", "rels"), new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
            new XElement(contentTypes + "Default", new XAttribute("Extension", "xml"), new XAttribute("ContentType", "application/xml")),
            new XElement(contentTypes + "Override", new XAttribute("PartName", "/xl/workbook.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
            new XElement(contentTypes + "Override", new XAttribute("PartName", "/xl/worksheets/sheet1.xml"), new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")))));
        Add("_rels/.rels", new XDocument(new XElement(relations + "Relationships", new XElement(relations + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", documentRelations.NamespaceName + "/officeDocument"), new XAttribute("Target", "xl/workbook.xml")))));
        Add("xl/workbook.xml", new XDocument(new XElement(spreadsheet + "workbook", new XElement(spreadsheet + "sheets", new XElement(spreadsheet + "sheet", new XAttribute("name", "Vocabulary"), new XAttribute("sheetId", "1"), new XAttribute(documentRelations + "id", "rId1"))))));
        Add("xl/_rels/workbook.xml.rels", new XDocument(new XElement(relations + "Relationships", new XElement(relations + "Relationship", new XAttribute("Id", "rId1"), new XAttribute("Type", documentRelations.NamespaceName + "/worksheet"), new XAttribute("Target", "worksheets/sheet1.xml")))));
        var values = new[] { (Vietnamese: "Tiếng Việt", English: "English") }.Concat(cards.Select(card => (card.Vietnamese, card.English)));
        var rows = values.Select((pair, index) => new XElement(spreadsheet + "row", new XAttribute("r", index + 1),
            new XElement(spreadsheet + "c", new XAttribute("r", $"A{index + 1}"), new XAttribute("t", "inlineStr"), new XElement(spreadsheet + "is", new XElement(spreadsheet + "t", pair.Vietnamese))),
            new XElement(spreadsheet + "c", new XAttribute("r", $"B{index + 1}"), new XAttribute("t", "inlineStr"), new XElement(spreadsheet + "is", new XElement(spreadsheet + "t", pair.English)))));
        Add("xl/worksheets/sheet1.xml", new XDocument(new XElement(spreadsheet + "worksheet", new XElement(spreadsheet + "sheetData", rows))));
    }
}
