using System.ComponentModel.DataAnnotations;

public class BankUploadFileVM
{
    [Required]
    public string FileInfoCode { get; set; }

    [Required]
    public string ResponseFile { get; set; }

    public string AcknowledgmentFile { get; set; }

    public string NoAcknowledgmentFile { get; set; }

    public string ResponseFileName { get; set; }

    public string AcknowledgmentFileName { get; set; }

    public string NoAcknowledgmentFileName { get; set; }
}
