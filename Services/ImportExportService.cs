using System.Globalization;
using System.Text;
using HouseholdBook.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace HouseholdBook.Services
{
    public interface IImportExportService
    {
        Task<byte[]> ExportTransactionsToCsvAsync(DateTime? from = null, DateTime? to = null);
        Task<byte[]> ExportTransactionsToWorksheetAsync(DateTime? from = null, DateTime? to = null);
        Task ImportTransactionsFromCsvAsync(Stream csvStream);
    }

    public class ImportExportService(ApplicationDbContext context) : IImportExportService
    {
        public async Task<byte[]> ExportTransactionsToCsvAsync(DateTime? from = null, DateTime? to = null)
        {
            var transactions = await FilterTransactions(from, to).ToListAsync();
            var builder = new StringBuilder();
            builder.AppendLine("Date,Amount,Category,Account,Description");
            foreach (var transaction in transactions)
            {
                builder.AppendLine(string.Join(',',
                    transaction.Date.ToString("yyyy-MM-dd"),
                    transaction.Amount.ToString(CultureInfo.InvariantCulture),
                    Escape(transaction.Category?.Name),
                    Escape(transaction.Account?.Name),
                    Escape(transaction.Description)));
            }
            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        public async Task<byte[]> ExportTransactionsToWorksheetAsync(DateTime? from = null, DateTime? to = null)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Transactions");
            worksheet.Cells[1, 1].Value = "Date";
            worksheet.Cells[1, 2].Value = "Amount";
            worksheet.Cells[1, 3].Value = "Category";
            worksheet.Cells[1, 4].Value = "Account";
            worksheet.Cells[1, 5].Value = "Description";

            var transactions = await FilterTransactions(from, to).ToListAsync();
            var row = 2;
            foreach (var transaction in transactions)
            {
                worksheet.Cells[row, 1].Value = transaction.Date;
                worksheet.Cells[row, 1].Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Cells[row, 2].Value = transaction.Amount;
                worksheet.Cells[row, 3].Value = transaction.Category?.Name;
                worksheet.Cells[row, 4].Value = transaction.Account?.Name;
                worksheet.Cells[row, 5].Value = transaction.Description;
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        public async Task ImportTransactionsFromCsvAsync(Stream csvStream)
        {
            using var reader = new StreamReader(csvStream, Encoding.UTF8, leaveOpen: false);
            var header = await reader.ReadLineAsync();
            if (header == null)
            {
                return;
            }

            var categories = await context.Categories.ToListAsync();
            var accounts = await context.Accounts.ToListAsync();
            Category? defaultCategory = categories.FirstOrDefault();
            Account? defaultAccount = accounts.FirstOrDefault();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var columns = ParseCsvLine(line);
                if (columns.Length < 5)
                {
                    continue;
                }

                if (!DateTime.TryParse(columns[0], out var date))
                {
                    continue;
                }

                if (!decimal.TryParse(columns[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                {
                    continue;
                }

                var categoryName = columns[2];
                var category = categories.FirstOrDefault(c => string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase));
                if (category == null && !string.IsNullOrWhiteSpace(categoryName))
                {
                    category = new Category { Name = categoryName, Type = amount >= 0 ? "Income" : "Expense" };
                    context.Categories.Add(category);
                    categories.Add(category);
                }
                defaultCategory ??= category;

                var accountName = columns[3];
                var account = accounts.FirstOrDefault(a => string.Equals(a.Name, accountName, StringComparison.OrdinalIgnoreCase));
                if (account == null && !string.IsNullOrWhiteSpace(accountName))
                {
                    account = new Account { Name = accountName, Type = "Cash" };
                    context.Accounts.Add(account);
                    accounts.Add(account);
                }
                defaultAccount ??= account;

                var transaction = new Transaction
                {
                    Date = date,
                    Amount = amount,
                    Description = columns[4],
                    Category = category ?? defaultCategory ?? new Category { Name = amount >= 0 ? "その他収入" : "その他支出", Type = amount >= 0 ? "Income" : "Expense" },
                    Account = account ?? defaultAccount ?? new Account { Name = "現金", Type = "Cash" }
                };
                if (transaction.Category.Id == 0)
                {
                    categories.Add(transaction.Category);
                    context.Categories.Add(transaction.Category);
                }
                if (transaction.Account.Id == 0)
                {
                    accounts.Add(transaction.Account);
                    context.Accounts.Add(transaction.Account);
                }
                context.Transactions.Add(transaction);
            }

            await context.SaveChangesAsync();
        }

        private IQueryable<Transaction> FilterTransactions(DateTime? from, DateTime? to)
        {
            var query = context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(t => t.Date >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(t => t.Date <= to.Value);
            }

            return query.OrderBy(t => t.Date);
        }

        private static string Escape(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            if (input.Contains(',') || input.Contains('"'))
            {
                return $"\"{input.Replace("\"", "\"\"")}\"";
            }
            return input;
        }

        private static string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            values.Add(current.ToString());
            return values.ToArray();
        }
    }
}
