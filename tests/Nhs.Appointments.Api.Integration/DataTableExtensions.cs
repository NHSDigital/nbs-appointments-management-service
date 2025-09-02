using System;
using System.Linq;
using Gherkin.Ast;

namespace Nhs.Appointments.Api.Integration;

public static class DataTableExtensions
{
    public static string GetRowValueOrDefault(this DataTable dataTable, TableRow row, string columnName,
        string defaultValue = null)
    {
        var columnLocation = dataTable.Rows.ElementAt(0).Cells.SingleOrDefault(cell => cell.Value == columnName)
            ?.Location.Column;
        var cell = row.Cells.SingleOrDefault(cell => cell.Location.Column == columnLocation)?.Value;

        return HasValue(cell) ? cell : defaultValue;
    }

    public static T? GetEnumRowValueOrDefault<T>(this DataTable dataTable, TableRow row, string columnName)
        where T : struct
    {
        var columnLocation = dataTable.Rows.ElementAt(0).Cells.SingleOrDefault(cell => cell.Value == columnName)
            ?.Location.Column;
        var cell = row.Cells.SingleOrDefault(cell => cell.Location.Column == columnLocation)?.Value;

        return HasValue(cell) ? Enum.Parse<T>(cell) : null;
    }

    public static T GetEnumRowValue<T>(this DataTable dataTable, TableRow row, string columnName,
        T defaultValue) where T : struct
    {
        var columnLocation = dataTable.Rows.ElementAt(0).Cells.SingleOrDefault(cell => cell.Value == columnName)
            ?.Location.Column;
        var cell = row.Cells.SingleOrDefault(cell => cell.Location.Column == columnLocation)?.Value;

        return HasValue(cell) ? Enum.Parse<T>(cell) : defaultValue;
    }

    public static string[] GetListRowValueOrDefault(this DataTable dataTable, TableRow row, string columnName,
        string[] defaultValue = null)
    {
        var columnLocation = dataTable.Rows.ElementAt(0).Cells.SingleOrDefault(cell => cell.Value == columnName)
            ?.Location.Column;
        var cell = row.Cells.SingleOrDefault(cell => cell.Location.Column == columnLocation)?.Value;

        return HasValue(cell) ? cell.Split(',').Select(s => s.Trim()).ToArray() : defaultValue;
    }

    public static T[] GetEnumListRowValueOrDefault<T>(this DataTable dataTable, TableRow row, string columnName,
        T[] defaultValue = null) where T : struct
    {
        var columnLocation = dataTable.Rows.ElementAt(0).Cells.SingleOrDefault(cell => cell.Value == columnName)
            ?.Location.Column;
        var cell = row.Cells.SingleOrDefault(cell => cell.Location.Column == columnLocation)?.Value;

        return HasValue(cell) ? cell.Split(',').Select(s => Enum.Parse<T>(s.Trim())).ToArray() : defaultValue;
    }

    private static bool HasValue(string candidate) => !string.IsNullOrEmpty(candidate) && candidate != "null";
}
