using System.Buffers.Text;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using API.Data.IRepositories;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;

        public Repository(ApplicationDbContext context)

        {
            _context = context;
            dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(
         Expression<Func<T, bool>> filter = null,
         Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
         string includeProperties = null)
        {
            try
            {
                IQueryable<T> query = dbSet;

                if (filter != null) query = query.Where(filter);

                if (includeProperties != null)
                {
                    foreach (var includeProp in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProp);
                    }
                }
                if (orderBy != null) query = orderBy(query);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }



        public async Task<T> FirstOrDefaultAsync(
           Expression<Func<T, bool>> filter = null,
           string includeProperties = null)
        {
            try
            {
                IQueryable<T> query = dbSet;

                if (filter != null)
                    query = query.Where(filter);

                if (includeProperties != null)
                {
                    foreach (var includeProp in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(includeProp); 
                    }
                }
                return await query.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }


        public async Task AddAsync(T entity)
        {
            try
            {
                await dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }


        }
        public async Task UpdateAsync(string entityCode, Func<T, Task> updateAction)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var entity = await GetAsync(entityCode);

                if (entity != null)
                {
                    await updateAction(entity);
                    _context.Entry(entity).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task UpdateByIntAsync(int entityCode, Func<T, Task> updateAction)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var entity = await GetByIntAsync(entityCode);

                if (entity != null)
                {
                    await updateAction(entity);
                    _context.Entry(entity).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }
        public async Task<T> GetByIntAsync(int code)
        {
            try
            {
                return await dbSet.FindAsync(code);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task DeleteAsync(int id)
        {
            var entity = await dbSet.FindAsync(id);
            if (entity != null) dbSet.Remove(entity);
        }
        public async Task<T> GetAsync(string code)
        {
            try
            {
                return await dbSet.FindAsync(code);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task RemoveAsync(T entity)
        {
            try
            {
                dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }
        public async Task RemoveAsync(string code)
        {
            try
            {
                var entity = await GetAsync(code);
                if (entity != null)
                {
                    await RemoveAsync(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }

        public async Task RemoveByIntAsync(int code)
        {
            try
            {
                var entity = await GetByIntAsync(code);
                if (entity != null)
                {
                    await RemoveAsync(entity);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }

        public async Task RemoveRangeAsync(IEnumerable<T> values)
        {
            try
            {
                dbSet.RemoveRange(values);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }

        public string GenrateUniqueCode()
        {
            return Guid.NewGuid().ToString();
        }

        public string DecrypteBase64(string value)
        {
            byte[] decodedBytes = Convert.FromBase64String(value);
            string decodedString = Encoding.UTF8.GetString(decodedBytes);
            return decodedString;
        }
        public async Task<string> GetFileAsBase64Async(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                return null;
            byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
            string base64 = Convert.ToBase64String(fileBytes);

            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            string mimeType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            return $"data:{mimeType};base64,{base64}";
        }

        public async Task<string> SaveReportFile(string documentionFile)
        {
            string directoryPath = "Data/TestResult";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            int commaIndex = documentionFile.IndexOf(',');
            string base64Data = documentionFile;
            string extension = ".bin";
            if (commaIndex >= 0)
            {
                string prefix = documentionFile.Substring(0, commaIndex);
                base64Data = documentionFile.Substring(commaIndex + 1);

                if (prefix.Contains("image/png"))
                    extension = ".png";
                else if (prefix.Contains("image/jpeg"))
                    extension = ".jpg";
                else if (prefix.Contains("application/pdf"))
                    extension = ".pdf";
            }

            byte[] fileBytes = Convert.FromBase64String(base64Data);

            string filePath = Path.Combine(directoryPath, $"{Guid.NewGuid()}{extension}");

            await File.WriteAllBytesAsync(filePath, fileBytes);

            return filePath;
        }

        //public IFormFile DecrypteIFromFileBase64(string value)
        //{
        //    if (string.IsNullOrWhiteSpace(value))
        //        throw new ArgumentException("Base64 string is empty");

        //    string base64Data = value;

        //    // If data URL, strip prefix
        //    if (value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        //    {
        //        base64Data = value.Substring(value.IndexOf(",") + 1);
        //    }

        //    byte[] fileBytes;
        //    try
        //    {
        //        fileBytes = Convert.FromBase64String(base64Data);
        //    }
        //    catch
        //    {
        //        throw new Exception("Invalid Base64 string");
        //    }

        //    // Detect file type by magic numbers
        //    string extension = fileBytes switch
        //    {
        //        var b when b.Length > 4 && b[0] == 0x50 && b[1] == 0x4B => ".xlsx", // ZIP header = XLSX
        //        var b when b.Length > 8 && b[0] == 0xD0 && b[1] == 0xCF => ".xls",  // Old Excel
        //        _ => ".csv" // fallback
        //    };

        //    var stream = new MemoryStream(fileBytes);

        //    return new FormFile(stream, 0, fileBytes.Length, "file", $"{Guid.NewGuid()}{extension}");
        //}
        //public IFormFile DecrypteIFromFileBase64(string value)
        //{
        //    if (string.IsNullOrWhiteSpace(value))
        //        throw new ArgumentException("Base64 string is empty");

        //    string base64Data = value;

        //    // Strip data URL prefix
        //    if (value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        //    {
        //        base64Data = value.Substring(value.IndexOf(",") + 1);
        //    }

        //    byte[] fileBytes;
        //    try
        //    {
        //        fileBytes = Convert.FromBase64String(base64Data);
        //    }
        //    catch
        //    {
        //        throw new Exception("Invalid Base64 string");
        //    }

        //    string extension;

        //    // XLS (OLE)
        //    if (fileBytes.Length > 8 &&
        //        fileBytes[0] == 0xD0 && fileBytes[1] == 0xCF)
        //    {
        //        extension = ".xls";
        //    }
        //    // ZIP (PK)
        //    else if (fileBytes.Length > 4 &&
        //             fileBytes[0] == 0x50 && fileBytes[1] == 0x4B)
        //    {
        //        // ZIP ko ZIP hi rehne do
        //        extension = ".zip";
        //    }
        //    else
        //    {
        //        extension = ".csv";
        //    }

        //    var stream = new MemoryStream(fileBytes);

        //    return new FormFile(
        //        stream,
        //        0,
        //        fileBytes.Length,
        //        "file",
        //        $"{Guid.NewGuid()}{extension}"
        //    );
        //}

        public IFormFile DecrypteIFromFileBase64(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Base64 string is empty");

            string base64Data = value;

            // Strip data URL prefix
            if (value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                base64Data = value.Substring(value.IndexOf(",") + 1);

            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(base64Data);
            }
            catch
            {
                throw new Exception("Invalid Base64 string");
            }

            string extension;

            // 1️⃣ XLS (Excel 97–2003)
            if (fileBytes.Length > 8 &&
                fileBytes[0] == 0xD0 && fileBytes[1] == 0xCF)
            {
                extension = ".xls";
            }
            // 2️⃣ ZIP based (xlsx / zip)
            else if (fileBytes.Length > 4 &&
                     fileBytes[0] == 0x50 && fileBytes[1] == 0x4B)
            {
                using var ms = new MemoryStream(fileBytes);
                using var zip = new ZipArchive(ms, ZipArchiveMode.Read, true);

                // Excel (.xlsx)
                if (zip.Entries.Any(e => e.FullName.StartsWith("xl/")))
                    extension = ".xlsx";
                else
                    extension = ".zip";
            }
            // 3️⃣ Plain text → CSV (inline detection)
            else
            {
                bool isText = true;
                int checkLength = Math.Min(fileBytes.Length, 512);

                for (int i = 0; i < checkLength; i++)
                {
                    if (fileBytes[i] == 0x00)
                    {
                        isText = false;
                        break;
                    }
                }

                extension = isText ? ".csv" : ".bin";
            }

            var stream = new MemoryStream(fileBytes);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var formFile = new FormFile(stream, 0, fileBytes.Length, "file", fileName);

            // ✅ Detect content type from file extension
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream"; // fallback
            }

            formFile.Headers = new HeaderDictionary();
            formFile.Headers["Content-Type"] = contentType;

            return formFile;
        }
        public IFormFile DecrypteIFromFileWithFileNameBase64(string value, string filename)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Base64 string is empty");

            string base64Data = value;

            // Strip data URL prefix
            if (value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                base64Data = value.Substring(value.IndexOf(",") + 1);

            byte[] fileBytes;
            try
            {
                fileBytes = Convert.FromBase64String(base64Data);
            }
            catch
            {
                throw new Exception("Invalid Base64 string");
            }

            string extension;

            // 1️⃣ XLS (Excel 97–2003)
            if (fileBytes.Length > 8 &&
                fileBytes[0] == 0xD0 && fileBytes[1] == 0xCF)
            {
                extension = ".xls";
            }
            // 2️⃣ ZIP based (xlsx / zip)
            else if (fileBytes.Length > 4 &&
                     fileBytes[0] == 0x50 && fileBytes[1] == 0x4B)
            {
                using var ms = new MemoryStream(fileBytes);
                using var zip = new ZipArchive(ms, ZipArchiveMode.Read, true);

                // Excel (.xlsx)
                if (zip.Entries.Any(e => e.FullName.StartsWith("xl/")))
                    extension = ".xlsx";
                else
                    extension = ".zip";
            }
            // 3️⃣ Plain text → CSV (inline detection)
            else
            {
                bool isText = true;
                int checkLength = Math.Min(fileBytes.Length, 512);

                for (int i = 0; i < checkLength; i++)
                {
                    if (fileBytes[i] == 0x00)
                    {
                        isText = false;
                        break;
                    }
                }

                extension = isText ? ".csv" : ".bin";
            }

            var stream = new MemoryStream(fileBytes);

            var formFile = new FormFile(stream, 0, fileBytes.Length, "file", filename);

            // ✅ Detect content type from file extension
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filename, out var contentType))
            {
                contentType = "application/octet-stream"; // fallback
            }

            formFile.Headers = new HeaderDictionary();
            formFile.Headers["Content-Type"] = contentType;

            return formFile;
        }

        public async Task<string> GenerateFileNumberAsync()
        {
            var today = DateTime.Now.ToString("yyyyMMdd");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var lastFileNumber = await _context.UploadFileInfo
                    .Where(x => x.FileNumber.StartsWith($"FILE-{today}"))
                    .OrderByDescending(x => x.FileNumber)
                    .Select(x => x.FileNumber)
                    .FirstOrDefaultAsync();

                int nextSequence = 1;

                if (!string.IsNullOrEmpty(lastFileNumber))
                {
                    var lastSeq = int.Parse(lastFileNumber.Split('-').Last());
                    nextSequence = lastSeq + 1;
                }

                var newFileNumber = $"FILE-{today}-{nextSequence:D4}";

                await transaction.CommitAsync();
                return newFileNumber;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }

}
