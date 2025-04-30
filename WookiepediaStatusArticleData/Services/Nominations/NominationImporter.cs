using System.Globalization;
using CsvHelper;
using CsvHelper.TypeConversion;
using Microsoft.EntityFrameworkCore;
using WookiepediaStatusArticleData.Database;
using WookiepediaStatusArticleData.Nominations.Nominations.Csv;
using WookiepediaStatusArticleData.Nominations.Nominators;
using WookiepediaStatusArticleData.Nominations.Projects;

namespace WookiepediaStatusArticleData.Services.Nominations;

public class NominationImporter(
    WookiepediaDbContext db,
    NominationCsvRowProcessor rowProcessor
)
{
    public async Task ExecuteAsync(
        Stream fileStream,
        CancellationToken cancellationToken
    )
    {
        var projects = await db.Set<Project>().ToDictionaryAsync(it => it.Name, cancellationToken);
        var nominators = await db.Set<Nominator>().ToDictionaryAsync(it => it.Name, cancellationToken);

        try
        {
            using var streamReader = new StreamReader(fileStream);
            using var csv = new CsvReader(streamReader, CultureInfo.InvariantCulture);
            csv.Context.RegisterClassMap<NominationCsvRowClassMap>();

            var rows = csv.GetRecordsAsync<NominationCsvRow>(cancellationToken);

            await foreach (var row in rows)
            {
                var nomination = rowProcessor.Convert(row, projects, nominators);
                db.Add(nomination);
            }
        }
        catch (ReaderException ex)
        {
            var column = ex.Context?.Reader?.HeaderRecord?[ex.Context.Reader.CurrentIndex];
            var rowNumber = ex.Context?.Parser?.Row;
            var invalidValue = ex.Message;

            throw new ValidationException(
                "",
                $"Row {rowNumber}, Column {column}: failed to parse due to the following message: \"{invalidValue}\"."
            );
        }
        catch (HeaderValidationException ex)
        {
            var headerNames = string.Join(", ", ex.InvalidHeaders.SelectMany(invalidHeader => invalidHeader.Names));

            throw new ValidationException(
                "",
                $"The provided CSV file is missing the following header columns: {headerNames}"
            );
        }
        catch (TypeConverterException ex)
        {
            var column = ex.Context?.Reader?.HeaderRecord?[ex.Context.Reader.CurrentIndex];
            var rowNumber = ex.Context?.Parser?.Row;
            var invalidValue = ex.Text;

            throw new ValidationException(
                "",
                $"Row {rowNumber}: \"{invalidValue}\" is not a valid value for {column}."
            );
        }
        catch (FieldValidationException ex)
        {
            var column = ex.Context?.Reader?.HeaderRecord?[ex.Context.Reader.CurrentIndex];
            var rowNumber = ex.Context?.Parser?.Row;
            var invalidValue = ex.Field;

            throw new ValidationException(
                "",
                $"Row {rowNumber}: \"{invalidValue}\" is not a valid value for {column}."
            );
        }
        catch (BadDataException)
        {
            throw new ValidationException(
                "",
                "Failed to read the CSV. This can happen if your CSV isn't formatted properly or uploaded a different file type (like an XLSX)."
            );
        }
    }
}