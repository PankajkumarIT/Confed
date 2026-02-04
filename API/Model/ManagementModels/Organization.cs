using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model.ManagementModels
{
    public class Organization
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Key]
        public string OrganizationCode { get; set; }
        [Required]
        public string OrganizationName { get; set; }
        public string ShortName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string ContactNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [RegularExpression("^(department|transporter|bank)$", ErrorMessage = "Invalid Organization Type.")]
        public string OrganizationType { get; set; } = null;
        public string ParentOrganizationCode { get; set; }
        [RegularExpression("^(SFTP|SFTPForPaymentTransaction|SFTPForPaymentWithEncryption)$", ErrorMessage = "Invalid Feture Type.")]
        public string FetureType { get; set; }
        public string GstNumber {  get; set; }
        public string PanNumber {  get; set; }

        public double StorageSize { get; set; }
        public double UsedStorageSize { get; set; }
        public double AllocateStorageSize { get; set; }
        public string PaymentFileHeader { get; set; }
        public string PaymentResponseFileHeader { get; set; }
        public string PaymentAcknowledgment { get; set; }
        public string PaymentNotAcknowledgement { get; set; }
        public bool IsActive {  get; set; }
        public bool IsConversion {  get; set; }


    }
}
