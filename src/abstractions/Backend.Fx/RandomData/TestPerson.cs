namespace Backend.Fx.RandomData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TestPerson
    {
        public enum Genders
        {
            Male,
            Female
        }

        private static readonly Dictionary<string, string> InvalidCharacterReplacements = new Dictionary<string, string> {
            { "À", "A" },
            { "Á", "A" },
            { "Â", "A" },
            { "Ã", "A" },
            { "Ä", "A" },
            { "Å", "A" },
            { "Æ", "A" },
            { "Ç", "C" },
            { "È", "E" },
            { "É", "E" },
            { "Ê", "E" },
            { "Ë", "E" },
            { "Ì", "I" },
            { "Í", "I" },
            { "Î", "I" },
            { "Ï", "I" },
            { "Ð", "D" },
            { "Ñ", "N" },
            { "Ò", "O" },
            { "Ó", "O" },
            { "Ô", "O" },
            { "Õ", "O" },
            { "Ö", "O" },
            { "×", "x" },
            { "Ø", "O" },
            { "Ù", "U" },
            { "Ú", "U" },
            { "Û", "U" },
            { "Ü", "U" },
            { "Ý", "Y" },
            { "Þ", "p" },
            { "ß", "ss" },
            { "à", "a" },
            { "á", "a" },
            { "â", "a" },
            { "ã", "a" },
            { "ä", "a" },
            { "å", "a" },
            { "æ", "a" },
            { "ç", "c" },
            { "è", "e" },
            { "é", "e" },
            { "ê", "e" },
            { "ë", "e" },
            { "ì", "i" },
            { "í", "i" },
            { "î", "i" },
            { "ï", "i" },
            { "ð", "o" },
            { "ñ", "n" },
            { "ò", "o" },
            { "ó", "o" },
            { "ô", "o" },
            { "õ", "o" },
            { "ö", "o" },
            { "÷", "" },
            { "ø", "o" },
            { "ù", "" },
            { "ú", "u" },
            { "û", "u" },
            { "ü", "u" },
            { "ý", "y" },
            { "þ", "p" },
            { "ÿ", "y" }
        };

        private readonly string _email;

        public TestPerson(string firstName, string middleName, string lastName, string title, Genders gender, DateTime dateOfBirth, string email = null)
        {
            _email = email;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            Title = title;
            Gender = gender;
            DateOfBirth = dateOfBirth;
        }

        public DateTime DateOfBirth { get; }

        public string FirstName { get; }

        public Genders Gender { get; }

        public string LastName { get; }

        public int Age
        {
            get {
                var today = DateTime.Today;
                var age = today.Year - DateOfBirth.Year;
                if (DateOfBirth > today.AddYears(-age))
                {
                    age--;
                }

                return age;
            }
        }

        public string MiddleName { get; }

        public string Title { get; }

        public string Email => _email ?? $"{UserName}@no-email.not";

        public string UserName => SanitizeForUserName(FirstName) + "." + SanitizeForUserName(LastName);

        private static string SanitizeForUserName(string s)
        {
            s = new string((s.Where(char.IsLetterOrDigit).ToArray()));
            foreach (var invalidCharacterReplacement in InvalidCharacterReplacements)
            {
                s = s.Replace(invalidCharacterReplacement.Key, invalidCharacterReplacement.Value);
            }
            return s;
        }
    }
}