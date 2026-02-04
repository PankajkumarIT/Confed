using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Model.ViewModels
{
    public class RegisterOrganizationVM
    {
        public string OrganizationCode { get; set; }
        [Required]
        public string OrganizationName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string ContactNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        [RegularExpression("^(department|transporter|bank)$", ErrorMessage = "Invalid Organization Type.")]
        public string OrganizationType { get; set; } = null;
        public string ParentOrganizationCode { get; set; }
        [RegularExpression("^(SFTP|SFTPForPaymentTransaction|SFTPForPaymentWithEncryption)$", ErrorMessage = "Invalid F Type.")]
        public string FetureType {  get; set; }
        public string GstNumber { get; set; }
        public string PanNumber { get; set; }
        public string ShortName { get; set; }

        public double StorageSize { get; set; }
        public string PaymentFileHeader { get; set; }
        public string PaymentResponseFileHeader { get; set; }
        public string PaymentAcknowledgment { get; set; }
        public string PaymentNotAcknowledgement { get; set; }
        public bool IsConversion { get; set; }

        public bool IsActive { get; set; }

    }
}