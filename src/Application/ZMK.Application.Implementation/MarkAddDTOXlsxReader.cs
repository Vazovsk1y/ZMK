using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Entities;

namespace ZMK.Application.Implementation;

public class MarkAddDTOXlsxReader : IXlsxReader<MarkAddDTO>
{
    private const int OrderColumnIndex = 0;

    private const int CodeColumnIndex = 1;

    private const int TitleColumnIndex = 2;

    private const int CountColumnIndex = 3;

    private const int WeightColumnIndex = 4;

    public IEnumerable<MarkAddDTO> Read(string filePath, Guid projectId)
    {
        var fileInfo = new FileInfo(filePath);
        using var workbook = new XSSFWorkbook(fileInfo.OpenRead());

        if (workbook.NumberOfSheets == 0)
        {
            throw new InvalidDataException("В файле не обнаружено листов.");
        }

        for (int i = 0; i < workbook.NumberOfSheets; i++)
        {
            var currentSheet = workbook.GetSheetAt(i);
            if (currentSheet.PhysicalNumberOfRows == 0)
            {
                throw new InvalidDataException("Документ содержит пустой лист.");
            }

            // skip columns titles
            for (int j = 1; j < currentSheet.PhysicalNumberOfRows; j++)
            {
                yield return ToMark(currentSheet.GetRow(j), i, j, projectId);
            }
        }
    }

    private static MarkAddDTO ToMark(IRow row, int sheetIndex, int rowIndex, Guid projectId)
    {
        var cells = row.Cells;
        int order = int.TryParse(cells[OrderColumnIndex].NumericCellValue.ToString(), out var result) 
            ? result : throw new InvalidDataException($"Неккоректные данные. Страница: {sheetIndex}, строка: {rowIndex}, колонка: {OrderColumnIndex}.");
        string code = !string.IsNullOrWhiteSpace(cells[CodeColumnIndex].StringCellValue) 
            ? cells[CodeColumnIndex].StringCellValue : throw new InvalidDataException($"Неккоректные данные. Страница: {sheetIndex}, строка: {rowIndex}, колонка: {CodeColumnIndex}.");
        string title = !string.IsNullOrWhiteSpace(cells[TitleColumnIndex].StringCellValue)
            ? cells[TitleColumnIndex].StringCellValue : throw new InvalidDataException($"Неккоректные данные. Страница: {sheetIndex}, строка: {rowIndex}, колонка: {TitleColumnIndex}.");
        double count = cells[CountColumnIndex].NumericCellValue is double number && Mark.IsValidCount(number)
            ? cells[CountColumnIndex].NumericCellValue : throw new InvalidDataException($"Неккоректные данные. Страница: {sheetIndex}, строка: {rowIndex}, колонка: {CountColumnIndex}.");
        double weight = cells[WeightColumnIndex].NumericCellValue > 0
            ? cells[WeightColumnIndex].NumericCellValue : throw new InvalidDataException($"Неккоректные данные. Страница: {sheetIndex}, строка: {rowIndex}, колонка: {WeightColumnIndex}.");
        const string REMARK = "Марка была импортирована из таблицы эксель.";

        return new MarkAddDTO(
            projectId,
            code,
            title,
            order,
            weight,
            count,
            REMARK
            );
    }
}