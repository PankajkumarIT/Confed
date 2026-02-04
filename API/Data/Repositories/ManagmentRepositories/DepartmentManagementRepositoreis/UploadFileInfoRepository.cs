using API.Data.IRepositories.IManagmentRepositories.IDepartmentManagementRepositories;
using API.Helpers.Models;
using API.Model.ManagementModels.BankManagement;
using API.Model.ManagementModels.DepartmentManagement;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using NPOI.SS.UserModel;
using System.Text;

namespace API.Data.Repositories.ManagmentRepositories.DepartmentManagementRepositoreis
{

    public class UploadFileInfoRepository : Repository<UploadFileInfo>, IUploadFileInfoRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly S3ServiceModel _s3ServiceModel;

        public UploadFileInfoRepository(ApplicationDbContext context, IOptions<S3ServiceModel> options) : base(context)
        {
            _s3ServiceModel = options.Value;
            _context = context;
        }

        public void ConvertFile(string sourceFileContent, string targetHeaderCsv, Dictionary<string, string> columnMapping, out string convertedFileContent)
        {
            var lines = sourceFileContent.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToList();
            if (!lines.Any())
            {
                convertedFileContent = string.Empty;
                return;
            }

            var sourceHeaders = lines[0].Split(',').Select(h => h.Trim().ToLower()).ToList();
            var sourceIndexMap = sourceHeaders.Select((h, i) => new { h, i }).ToDictionary(x => x.h, x => x.i);
            var targetHeaders = targetHeaderCsv.Split(',').Select(h => h.Trim()).ToList();
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", targetHeaders));
            for (int r = 1; r < lines.Count; r++)
            {
                var row = lines[r].Split(',');
                var outputRow = new List<string>();

                foreach (var targetHeader in targetHeaders)
                {
                    var key = targetHeader.ToLower();

                    if (
                        columnMapping.ContainsKey(key) &&
                        sourceIndexMap.ContainsKey(columnMapping[key])
                    )
                    {
                        int index = sourceIndexMap[columnMapping[key]];
                        outputRow.Add(index < row.Length ? row[index].Trim() : "");
                    }
                    else
                    {
                        outputRow.Add("");
                    }
                }

                sb.AppendLine(string.Join(",", outputRow));
            }

            convertedFileContent = sb.ToString();
        }

        public string ConvertFileBytes(byte[] sourceFileBytes, string targetHeaderCsv, Dictionary<string, string> columnMapping)
        {
            var sourceContent = Encoding.UTF8.GetString(sourceFileBytes);

            ConvertFile(sourceContent, targetHeaderCsv, columnMapping, out string convertedContent);

            return convertedContent;
        }

        public Task<(byte[] FileBytes, string FileName)> GetPasswordProtectedDownloadAsync(
           byte[] sourceFileBytes,
           string password,
           string targetHeaderCsv,
           string columnMappingString,
           string fileType
       )
        {
            var columnMapping = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(columnMappingString))
            {
                foreach (var pair in columnMappingString.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = pair.Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (kv.Length == 2)
                        columnMapping[kv[0].Trim()] = kv[1].Trim();
                }
            }

            string convertedContent;
            if (fileType.ToLower() == "csv")
            {
                var sourceContent = Encoding.UTF8.GetString(sourceFileBytes);
                ConvertFile(sourceContent, targetHeaderCsv, columnMapping, out convertedContent);
            }
            else
            {
                convertedContent = ConvertExcelBytes(sourceFileBytes, targetHeaderCsv, columnMapping, fileType);
            }
            var fileBytes = Encoding.UTF8.GetBytes(convertedContent);
            var zipBytes = ProtectFileWithPassword(fileBytes, password, $"File.{fileType}");

            return Task.FromResult((zipBytes, $"ConvertedFile.zip"));
        }

        public byte[] ProtectFileWithPassword(byte[] fileBytes, string password, string fileName)
        {
            using var ms = new MemoryStream();
            using (var zipStream = new ZipOutputStream(ms))
            {
                zipStream.SetLevel(9);
                zipStream.Password = password;

                var entry = new ZipEntry(fileName)
                {
                    DateTime = DateTime.Now
                };

                zipStream.PutNextEntry(entry);
                zipStream.Write(fileBytes, 0, fileBytes.Length);
                zipStream.CloseEntry();
            }
            return ms.ToArray();
        }
        public (string fileHeader, List<string> rows, string filetype) ReadFile(IFormFile file)
        {
            string ext = Path.GetExtension(file.FileName)?.ToLower();
            string fileHeader = "";
            var rows = new List<string>();
            string filetype = "";
            if (ext == ".csv")
            {
                using var reader = new StreamReader(file.OpenReadStream());
                fileHeader = reader.ReadLine();
                filetype = ext;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line))
                        rows.Add(line);
                }
            }
            else if (ext == ".xlsx" || ext == ".xls")
            {
                filetype = ext;

                using var stream = file.OpenReadStream();
                IWorkbook workbook = ext == ".xlsx"
                    ? new NPOI.XSSF.UserModel.XSSFWorkbook(stream)
                    : new NPOI.HSSF.UserModel.HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                var headerRow = sheet.GetRow(0);
                if (headerRow != null)
                {
                    var sb = new StringBuilder();
                    for (int i = 0; i < headerRow.LastCellNum; i++)
                        sb.Append(headerRow.GetCell(i)?.ToString() + ",");

                    if (sb.Length > 0) sb.Length--;
                    fileHeader = sb.ToString();
                }

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    var sb = new StringBuilder();
                    for (int j = 0; j < row.LastCellNum; j++)
                        sb.Append(row.GetCell(j)?.ToString() + ",");

                    if (sb.Length > 0) sb.Length--;
                    rows.Add(sb.ToString());
                }
            }
            else
            {
                fileHeader = "";
                rows = new List<string>();
            }

            return (fileHeader, rows, filetype);
        }
        public void ValidateHeaders(string uploadedHeader, string expectedHeader)
        {
            if (string.IsNullOrWhiteSpace(uploadedHeader) || string.IsNullOrWhiteSpace(expectedHeader))
                throw new Exception("Invalid");

            var uploaded = uploadedHeader.Split(',').Select(x => x.Trim().ToLower()).ToList();
            var expected = expectedHeader.Split(',').Select(x => x.Trim().ToLower()).ToList();

            bool isMatch = expected.All(h => uploaded.Contains(h));
            if (!isMatch)
                throw new Exception("Invalid");
        }
        public void ValidateRequiredFieldConsistency(
         List<string> rows,
         List<string> expectedHeaders,
         BankBranch branch
     )
        {
            var uploadedHeaders = rows.First()
                .Split(',')
                .Select(h => h.Trim().ToLower())
                .ToList();

            var headerIndexMap = uploadedHeaders
                .Select((h, i) => new { h, i })
                .ToDictionary(x => x.h, x => x.i);

            var requiredFields = new List<string> { "bankname", "branchname", "ifsc" };

            for (int i = 1; i < rows.Count; i++)
            {
                var values = rows[i].Split(',');

                foreach (var field in requiredFields)
                {
                    if (!headerIndexMap.ContainsKey(field))
                        continue;

                    var index = headerIndexMap[field];
                    string fileValue = values.Length > index ? values[index].Trim() : "";

                    string dbValue = field switch
                    {
                        "bankname" => branch.BankName,
                        "branchname" => branch.BranchName,
                        "ifsc" => branch.IFSC,
                        _ => ""
                    };

                    if (!string.Equals(fileValue, dbValue, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception(
                            $"Invalid {field} at row {i + 1}. Expected: '{dbValue}', Found: '{fileValue}'"
                        );
                    }
                }
            }
        }
        public string ConvertExcelBytes(
    byte[] fileBytes,
    string targetHeaderCsv,
    Dictionary<string, string> columnMapping,
    string fileType
)
        {
            using var stream = new MemoryStream(fileBytes);
            IWorkbook workbook = fileType.ToLower() switch
            {
                "xlsx" => new NPOI.XSSF.UserModel.XSSFWorkbook(stream),
                "xls" => new NPOI.HSSF.UserModel.HSSFWorkbook(stream),
                _ => throw new Exception("Unsupported Excel format")
            };

            var sheet = workbook.GetSheetAt(0);

            var headerRow = sheet.GetRow(0);
            var sourceHeaders = new List<string>();
            for (int i = 0; i < headerRow.LastCellNum; i++)
                sourceHeaders.Add(headerRow.GetCell(i)?.ToString().Trim().ToLower() ?? "");

            var sourceIndexMap = sourceHeaders
                .Select((h, i) => new { h, i })
                .ToDictionary(x => x.h, x => x.i);

            var targetHeaders = targetHeaderCsv.Split(',').Select(h => h.Trim()).ToList();

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", targetHeaders));

            for (int r = 1; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;

                var outputRow = new List<string>();
                foreach (var targetHeader in targetHeaders)
                {
                    var key = targetHeader.ToLower();
                    if (columnMapping.ContainsKey(key) && sourceIndexMap.ContainsKey(columnMapping[key]))
                    {
                        int index = sourceIndexMap[columnMapping[key]];
                        var cell = row.GetCell(index);
                        outputRow.Add(cell?.ToString().Trim() ?? "");
                    }
                    else
                    {
                        outputRow.Add("");
                    }
                }

                sb.AppendLine(string.Join(",", outputRow));
            }

            return sb.ToString();
        }

        public double GetFileStorage(byte[] fileBytes)
        {
            long bytes = fileBytes.Length;
            double size;
            string unit;

            if (bytes >= 1024L * 1024 * 1024)
            {
                size = bytes / (1024.0 * 1024.0 * 1024.0);
                unit = "GB";
            }
            else if (bytes >= 1024L * 1024)
            {
                size = bytes / (1024.0 * 1024.0);
                unit = "MB";
            }
            else
            {
                size = bytes / 1024.0;
                unit = "KB";
            }

            string readableSize = $"{Math.Round(size, 2)} {unit}";
            double fileSizeInGB = bytes / (1024.0 * 1024.0 * 1024.0);

            return fileSizeInGB;
        }
    }
}
