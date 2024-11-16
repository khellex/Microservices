using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utilities
{
    public class AllowedImageSizeAttribute : ValidationAttribute
    {
        //this is a specified extension type, that we will mention in the data annotation to be validated 
        private readonly int _maxImageSize;
        public AllowedImageSizeAttribute(int maxImageSize)
        {
            _maxImageSize = maxImageSize;
        }
        //this method can be utilized to create the custom data annotation
        //which we can then consume in any model or dto where we use the data annotation namespace
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                //here we will check the size of the uploaded image
                if (file.Length > ( _maxImageSize * 2048 * 2048))
                {
                    //if yes, we will display the following validation message
                    return new ValidationResult($"Maximum allowed image size is {_maxImageSize} MB.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
