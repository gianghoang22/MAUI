$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.IO.Compression

$sampleDirectory = Join-Path $PSScriptRoot '..\Samples'
$spreadsheetNamespace = 'http://schemas.openxmlformats.org/spreadsheetml/2006/main'
$relationshipNamespace = 'http://schemas.openxmlformats.org/officeDocument/2006/relationships'
$packageRelationshipNamespace = 'http://schemas.openxmlformats.org/package/2006/relationships'
$contentTypeNamespace = 'http://schemas.openxmlformats.org/package/2006/content-types'

function Add-XmlEntry($archive, [string] $name, [xml] $document) {
    $entry = $archive.CreateEntry($name)
    $stream = $entry.Open()
    try {
        $document.Save($stream)
    }
    finally {
        $stream.Dispose()
    }
}

function New-Document([string] $xml) {
    $document = [xml]::new()
    $document.LoadXml($xml)
    return $document
}

function Decode-Text([string] $value) {
    return [System.Text.RegularExpressions.Regex]::Unescape($value)
}

function Write-Workbook([string] $fileName, [string] $firstHeader, [string] $secondHeader, [string[]] $cells) {
    $filePath = Join-Path $sampleDirectory $fileName
    $fileStream = [System.IO.File]::Create($filePath)
    try {
        $archive = [System.IO.Compression.ZipArchive]::new($fileStream, [System.IO.Compression.ZipArchiveMode]::Create)
        try {
            Add-XmlEntry $archive '[Content_Types].xml' (New-Document @"
<Types xmlns="$contentTypeNamespace"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/></Types>
"@)
            Add-XmlEntry $archive '_rels/.rels' (New-Document @"
<Relationships xmlns="$packageRelationshipNamespace"><Relationship Id="rId1" Type="$relationshipNamespace/officeDocument" Target="xl/workbook.xml"/></Relationships>
"@)
            Add-XmlEntry $archive 'xl/workbook.xml' (New-Document @"
<workbook xmlns="$spreadsheetNamespace" xmlns:r="$relationshipNamespace"><sheets><sheet name="Vocabulary" sheetId="1" r:id="rId1"/></sheets></workbook>
"@)
            Add-XmlEntry $archive 'xl/_rels/workbook.xml.rels' (New-Document @"
<Relationships xmlns="$packageRelationshipNamespace"><Relationship Id="rId1" Type="$relationshipNamespace/worksheet" Target="worksheets/sheet1.xml"/></Relationships>
"@)

            $sheet = [xml]::new()
            $worksheet = $sheet.CreateElement('worksheet', $spreadsheetNamespace)
            $sheet.AppendChild($worksheet) | Out-Null
            $sheetData = $sheet.CreateElement('sheetData', $spreadsheetNamespace)
            $worksheet.AppendChild($sheetData) | Out-Null
            $rowCount = [Math]::Ceiling($cells.Count / 2) + 1
            for ($rowIndex = 0; $rowIndex -lt $rowCount; $rowIndex++) {
                $rowNumber = $rowIndex + 1
                $rowElement = $sheet.CreateElement('row', $spreadsheetNamespace)
                $rowElement.SetAttribute('r', [string] $rowNumber)
                $sheetData.AppendChild($rowElement) | Out-Null
                for ($columnIndex = 0; $columnIndex -lt 2; $columnIndex++) {
                    $column = if ($columnIndex -eq 0) { 'A' } else { 'B' }
                    $cell = $sheet.CreateElement('c', $spreadsheetNamespace)
                    $cell.SetAttribute('r', "$column$rowNumber")
                    $cell.SetAttribute('t', 'inlineStr')
                    $inlineString = $sheet.CreateElement('is', $spreadsheetNamespace)
                    $text = $sheet.CreateElement('t', $spreadsheetNamespace)
                    if ($rowIndex -eq 0) {
                        $value = if ($columnIndex -eq 0) { $firstHeader } else { $secondHeader }
                    }
                    else {
                        $cellIndex = (($rowIndex - 1) * 2) + $columnIndex
                        $value = if ($cellIndex -lt $cells.Count) { $cells[$cellIndex] } else { '' }
                    }
                    $text.InnerText = Decode-Text $value
                    $inlineString.AppendChild($text) | Out-Null
                    $cell.AppendChild($inlineString) | Out-Null
                    $rowElement.AppendChild($cell) | Out-Null
                }
            }

            Add-XmlEntry $archive 'xl/worksheets/sheet1.xml' $sheet
        }
        finally {
            $archive.Dispose()
        }
    }
    finally {
        $fileStream.Dispose()
    }
}

Write-Workbook 'VocabMate-template-daily-life.xlsx' 'Ti\u1ebfng Vi\u1ec7t' 'English' @(
    'bu\u1ed5i s\u00e1ng', 'morning', 'b\u1eefa s\u00e1ng', 'breakfast',
    '\u0111\u00e1nh th\u1ee9c', 'wake up', '\u0111\u00e1nh r\u0103ng', 'brush teeth',
    'chu\u1ea9n b\u1ecb', 'prepare', 'b\u1eadn r\u1ed9n', 'busy',
    'h\u00e0ng x\u00f3m', 'neighbor', 'si\u00eau th\u1ecb', 'supermarket',
    'n\u1ea5u \u0103n', 'cook', 'ngh\u1ec9 ng\u01a1i', 'rest'
)

Write-Workbook 'VocabMate-template-travel.xlsx' 'Ti\u1ebfng Vi\u1ec7t' 'English' @(
    's\u00e2n bay', 'airport', 'v\u00e9 m\u1ed9t chi\u1ec1u', 'one-way ticket',
    'h\u1ed9 chi\u1ebfu', 'passport', 'h\u00e0nh l\u00fd', 'luggage',
    'nh\u00e0 ga', 'train station', 'b\u1ea3n \u0111\u1ed3', 'map',
    '\u0111\u1eb7t ph\u00f2ng', 'book a room', '\u0111\u01b0\u1eddng ph\u1ed1', 'street',
    'r\u1ebd tr\u00e1i', 'turn left', '\u1edf g\u1ea7n \u0111\u00e2y', 'nearby'
)

Write-Workbook 'VocabMate-template-preview-cases.xlsx' 'Ti\u1ebfng Vi\u1ec7t' 'English' @(
    'qu\u1ea3 t\u00e1o', 'apple', 'qu\u1ea3 t\u00e1o', 'apple',
    'con ch\u00f3', 'dog', 'm\u1ed9t t\u1eeb ch\u01b0a d\u1ecbch', '',
    'xin ch\u00e0o', 'hello'
)