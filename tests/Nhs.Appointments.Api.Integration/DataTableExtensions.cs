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
        return columnLocation == null
            ? defaultValue
            : row.Cells.Single(cell => cell.Location.Column == columnLocation).Value;
    }

    public static T GetEnumRowValueOrDefault<T>(this DataTable dataTable, TableRow row, string columnName,
        T defaultValue) where T : struct
    {
        var columnLocation = dataTable.Rows.ElementAt(0).Cells.SingleOrDefault(cell => cell.Value == columnName)
            ?.Location.Column;

        return columnLocation == null
            ? defaultValue
            : Enum.Parse<T>(row.Cells.Single(cell => cell.Location.Column == columnLocation).Value);
    }
}
