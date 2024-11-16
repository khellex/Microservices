using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utilities
{
    public class AllowedExtensionAttribute : ValidationAttribute
    {
        //this is a specified extension type, that we will mention in the data annotation to be validated 
        private readonly string[] _extensions;
        public AllowedExtensionAttribute(string[] extensions)
        {
            _extensions = extensions;
        }
        //this method can be utilized to create the custom data annotation
        //which we can then consume in any model or dto where we use the data annotation namespace
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                //here we will check the extension of the new image file we just uploaded
                var extension = Path.GetExtension(file.FileName);

                //here we check if the uploaded image extension is of
                //the type we have mentioned in _extensions(to be allowed)
                if (!_extensions.Contains(extension.ToLower()))
                {
                    //if it is of the type not mentioned in the validation, then we will display the following validation message
                    return new ValidationResult("This image extension is not allowed.");
                }
            }
            return ValidationResult.Success;
        }
    }
}
