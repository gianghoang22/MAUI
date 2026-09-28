import argparse
from pathlib import Path
from xml.etree.ElementTree import Element, SubElement, tostring
from zipfile import ZipFile, ZIP_DEFLATED

WORDS = [("猫", "cat"), ("犬", "dog"), ("水", "water"), ("本", "book"),
         ("学校", "school"), ("太陽", "sun"), ("月", "moon"), ("花", "flower"),
         ("山", "mountain"), ("川", "river"), ("友達", "friend"), ("車", "car"), ("電車", "train")]


def write_workbook(path, headers, rows):
    spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"
    worksheet = Element("worksheet", xmlns=spreadsheet)
    sheet_data = SubElement(worksheet, "sheetData")
    for row_number, values in enumerate([headers, *rows], 1):
        row = SubElement(sheet_data, "row", r=str(row_number))
        for column, value in zip(("A", "B"), values):
            cell = SubElement(row, "c", r=f"{column}{row_number}", t="inlineStr")
            SubElement(SubElement(cell, "is"), "t").text = value
    with ZipFile(path, "w", ZIP_DEFLATED) as archive:
        archive.writestr("[Content_Types].xml", '<?xml version="1.0"?><Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/></Types>')
        archive.writestr("_rels/.rels", '<?xml version="1.0"?><Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>')
        archive.writestr("xl/workbook.xml", f'<workbook xmlns="{spreadsheet}" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="UI automation" sheetId="1" r:id="rId1"/></sheets></workbook>')
        archive.writestr("xl/_rels/workbook.xml.rels", '<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/></Relationships>')
        archive.writestr("xl/worksheets/sheet1.xml", tostring(worksheet, encoding="utf-8", xml_declaration=True))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("output", type=Path)
    output = parser.parse_args().output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    write_workbook(output / "japanese-english-13.xlsx", ("Japanese", "English"), WORDS)
    write_workbook(output / "reversed-duplicates.xlsx", ("English", "Japanese"), [(second, first) for first, second in WORDS])
    write_workbook(output / "invalid-row.xlsx", ("Japanese", "English"), [*WORDS[:2], ("鳥", "")])
    write_workbook(output / "language-mismatch.xlsx", ("French", "English"), [("livre", "book")])
    print(output)


if __name__ == "__main__":
    main()
