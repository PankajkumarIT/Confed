using API.Data.Repositories;
using API.Model.UserModels;
using API.Data.IRepository.UserRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using API.Data.IRepositories;

namespace API.Data.Repository.UserRepositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly AppSettings _appSettings;
        public UserRepository(ApplicationDbContext context, IOptions<AppSettings> appSettings) : base(context)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }
        public async Task<bool> ActiveDeactiveUser(string userCode)
        {
            try
            {
                var userindb = _context.User.FirstOrDefault(x => x.UserCode == userCode);
                if (userindb == null)
                {
                    return false;
                }
                else
                {
                    if (userindb.IsActive == true)
                        userindb.IsActive = false;
                    else
                        userindb.IsActive = true;

                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<string> Authenticate(string MobileNumber, string roleId)
        {
            try
            {
                var userindb = await dbSet.FirstOrDefaultAsync(x => x.MobileNumber == MobileNumber);
                if (userindb == null)
                    return null;

                string UserTypeCode = null;
                string Usertype = null;
                string organizationCode = null;
                string entityOrganizationMapCode = null;
                var adminUser = await _context.Administor
                    .FirstOrDefaultAsync(x => x.UserCode == userindb.UserCode);
                if (adminUser != null)
                {
                    organizationCode = adminUser.OrganizationCode;
                    UserTypeCode = adminUser.AdministorCode;
                    entityOrganizationMapCode = null;
                    Usertype = SD.AdminstratorUser;
                }
                var departmentUser = await _context.DepartmentUser.Include(x=>x.Office)
                    .FirstOrDefaultAsync(x => x.UserCode == userindb.UserCode);

                if (departmentUser != null)
                {
                    organizationCode = departmentUser.Office.OrganizationCode;
                    UserTypeCode = departmentUser.DepartmentUserCode;
                    Usertype = SD.DepartmentUser;
                    entityOrganizationMapCode = departmentUser.OfficeCode;
                }
                else
                {
                    var transporterUser = await _context.TransporterUser.FirstOrDefaultAsync(x => x.UserCode == userindb.UserCode);

                    if (transporterUser != null)
                    {
                        organizationCode = transporterUser.OrganizationCode;
                        UserTypeCode = transporterUser.TransportUserCode;
                        Usertype = SD.TransportUser;
                        entityOrganizationMapCode = null;
                    }
                    else
                    {
                        var bankUser = await _context.BankUser.Include(x=>x.BankBranch)
                            .FirstOrDefaultAsync(x => x.UserCode == userindb.UserCode);

                        if (bankUser != null)
                        {
                            organizationCode = bankUser.BankBranch.OrganizationCode;
                            Usertype= SD.BankUser;
                            UserTypeCode = bankUser.BankUserCode;
                            entityOrganizationMapCode = bankUser.BankBranchCode;
                        }
                    }
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescritor = new SecurityTokenDescriptor()
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                 new Claim(ClaimTypes.Role, roleId),
                 new Claim(ClaimTypes.SerialNumber, userindb.UserCode),
                 new Claim(ClaimTypes.MobilePhone, userindb.MobileNumber),
                 new Claim(SD.EntityUser, userindb.IsEntityUser? "true":"false"),
                 new Claim(SD.UserTypeCode, UserTypeCode ?? string.Empty),
                 new Claim(SD.UserType, Usertype ?? string.Empty),
                 new Claim(SD.OrganizationCode, organizationCode ?? string.Empty),
                 new Claim(SD.EntityCode, entityOrganizationMapCode ?? string.Empty),

                    }),
                    Expires = DateTime.Now.ToLocalTime().AddHours(24),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescritor);
                var Token = tokenHandler.WriteToken(token);
                return Token;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message); 
            }
        }
        public async Task<bool> IsUniqueUser(string MobileNumber)
        {
            try
            {
                var userindb = await dbSet.FirstOrDefaultAsync(x => x.MobileNumber == MobileNumber);
                if (userindb == null)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        public async Task<bool> RegisterUser(User User)
        {
            try
            {
                await dbSet.AddAsync(User);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}


