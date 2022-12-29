using Common.Command;
using Common.Data;
using Domain.Customer.Models;
using Domain.Models;
using Ramp.Contracts.CommandParameter.CustomerManagement;
using Ramp.Services.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LINQtoCSV;
using Domain.Enums;

namespace Ramp.Services.Validation.CustomerManagement
{
    public class ValidateCSVUploads : IValidator<SaveCsvCustomerCompanyUserCommand>
    {
        private readonly IRepository<RaceCodes> _raceCodeRepository;

        public ValidateCSVUploads(IRepository<RaceCodes> raceCodeRepository)
        {
            _raceCodeRepository = raceCodeRepository;
        }

        public IEnumerable<IValidationResult> Validate(SaveCsvCustomerCompanyUserCommand argument)
        {
            var validationResult = new List<IValidationResult>();
            try
            {

                if (argument.CsvHttpPostedFile != null && argument.CsvHttpPostedFile.ContentLength > 0)
                {

                    var path = argument.CsvFilePath;
                    var filePath = Path.Combine(path,
                        argument.CsvHttpPostedFile.FileName);
                    argument.CsvHttpPostedFile.SaveAs(filePath);
                    var descriptor = new CsvFileDescription { SeparatorChar = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator[0], FirstLineHasColumnNames = true };
                    var context = new CsvContext();
                    var users = context.Read<CSVMasks.User>(filePath, descriptor);

                    foreach (var u in users)
                    {
                        if (Validate(u.FullName, x => x.Length == 0))
                            validationResult.Add(new ValidationResult("CSV", "Name is not valid."));
                        if (Validate(u.EmailAddress, x => !IsValidEmail(x.Trim())))
                            validationResult.Add(new ValidationResult("CSV", "Email is invalid."));
                        if (Validate(u.Password, x => x.Length < 6))
                            validationResult.Add(new ValidationResult("CSV", "Password is invalid."));
                        if (Validate(u.MobileNumber, x => !IsDigitsOnly(x), false))
                            validationResult.Add(new ValidationResult("CSV", "Mobile Number is invalid."));
                        if (Validate(u.Group, x => x.Trim().Length == 0))
                            validationResult.Add(new ValidationResult("CSV", "No group spesified."));
                        if (Validate(u.Gender, delegate (string x)
                        {
                            if (x.Equals("m", StringComparison.InvariantCultureIgnoreCase))
                                x = GenderEnum.Gender.Male.ToString();
                            if (x.Equals("f", StringComparison.InvariantCultureIgnoreCase))
                                x = GenderEnum.Gender.Female.ToString();
                            GenderEnum.Gender g;
                            return !Enum.TryParse(x, true, out g);
                        },false))
                        {
                            validationResult.Add(new ValidationResult("CSV", "Invalid gender spesified."));
                        }
                        if (Validate(u.RaceCode, x => x.Length > 0,false))
                        {
                            if (!_raceCodeRepository.List.Any(
                                x => x.Code.TrimAllCastToLowerInvariant().Equals(u.RaceCode.TrimAllCastToLowerInvariant())
                                || x.Description.TrimAllCastToLowerInvariant().Equals(u.RaceCode.TrimAllCastToLowerInvariant())))
                            {
                                validationResult.Add(new ValidationResult("CSV", "Invalid racecode spesified."));
                            }
                        }
                    }
                }
                else
                {
                    validationResult.Add(new ValidationResult("CSV", "CSV is not valid"));
                }
                if (argument.CompanyId.Equals(Guid.Empty))
                    validationResult.Add(new ValidationResult("Command", "Company Id has not been set"));
                if (string.IsNullOrEmpty(argument.CsvFilePath))
                    validationResult.Add(new ValidationResult("Command", "CSV File Path has not been set"));
                if (argument.ParentUserId.Equals(Guid.Empty))
                    validationResult.Add(new ValidationResult("Command", "Parent User Id has not been set"));
                if (argument.UserRoles.Count <= 0)
                    validationResult.Add(new ValidationResult("Command", "No User Roles has been added"));
            }
            catch (Exception ex)
            {
                validationResult.Add(new ValidationResult("", ex.Message));
            }
            return validationResult;
        }

        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private bool Validate(string value,Func<string,bool> criteria,bool required = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true && required;
            else
                return criteria(value);
        }
    }
}