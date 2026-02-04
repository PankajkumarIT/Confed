using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels
{
    public class UploadFileInfoVM
    {
        public string UploadFileInfoCode {  get; set; }
        public string OrganizationCode { get; set; }        
        public string BankOrganizationCode { get; set; }
        public string OfficeCode {  get; set; }
        public string BankBranchCode {  get; set; }
        [Required]
        public string UploadFile {  get; set; }
        [Required]
        public bool IsDraft {  get; set; }
        public string FileName {  get; set; }
        public string AssignedToRoleCode { get; set; }
        public string AssignedToUserCode { get; set; }
        public string UserFileDirectoryCode {  get; set; }
        public bool IsSftpOnly { get; set; }
    }
}
