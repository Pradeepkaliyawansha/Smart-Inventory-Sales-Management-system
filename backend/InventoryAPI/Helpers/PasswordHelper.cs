using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace InventoryAPI.Helpers
{
    /// <summary>
    /// Provides secure password management utilities including hashing, verification, 
    /// generation, and strength validation
    /// </summary>
    public static class PasswordHelper
    {
        private const int DefaultSaltSize = 16;
        private const int DefaultHashSize = 32;
        private const int DefaultIterations = 100000; // PBKDF2 iterations
        private const int MinPasswordLength = 8;
        private const int MaxPasswordLength = 128;

        /// <summary>
        /// Hash a password using BCrypt (recommended for most use cases)
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="workFactor">BCrypt work factor (default: 12)</param>
        /// <returns>BCrypt hash string</returns>
        public static string HashPassword(string password, int workFactor = 12)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            if (password.Length > MaxPasswordLength)
                throw new ArgumentException($"Password cannot exceed {MaxPasswordLength} characters", nameof(password));

            if (workFactor < 4 || workFactor > 16)
                throw new ArgumentException("Work factor must be between 4 and 16", nameof(workFactor));

            return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
        }

        /// <summary>
        /// Verify a password against its BCrypt hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hash">BCrypt hash to verify against</param>
        /// <returns>True if password matches hash</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception)
            {
                // Invalid hash format or other BCrypt errors
                return false;
            }
        }

        /// <summary>
        /// Hash password using PBKDF2 with custom salt (alternative to BCrypt)
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="salt">Base64 encoded salt (if null, generates new salt)</param>
        /// <param name="iterations">Number of PBKDF2 iterations</param>
        /// <param name="hashSize">Size of hash in bytes</param>
        /// <returns>Tuple containing hash and salt (both Base64 encoded)</returns>
        public static (string hash, string salt) HashPasswordPBKDF2(
            string password, 
            string salt = null, 
            int iterations = DefaultIterations,
            int hashSize = DefaultHashSize)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            byte[] saltBytes;
            if (string.IsNullOrEmpty(salt))
            {
                saltBytes = GenerateSecureSalt(DefaultSaltSize);
            }
            else
            {
                try
                {
                    saltBytes = Convert.FromBase64String(salt);
                }
                catch (FormatException)
                {
                    throw new ArgumentException("Invalid salt format. Must be Base64 encoded.", nameof(salt));
                }
            }

            using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(hashSize);

            return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }

        /// <summary>
        /// Verify password against PBKDF2 hash
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="hash">Base64 encoded hash</param>
        /// <param name="salt">Base64 encoded salt</param>
        /// <param name="iterations">Number of PBKDF2 iterations used</param>
        /// <param name="hashSize">Size of hash in bytes</param>
        /// <returns>True if password matches</returns>
        public static bool VerifyPasswordPBKDF2(
            string password, 
            string hash, 
            string salt,
            int iterations = DefaultIterations,
            int hashSize = DefaultHashSize)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
                return false;

            try
            {
                var saltBytes = Convert.FromBase64String(salt);
                var hashBytes = Convert.FromBase64String(hash);

                using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256);
                var computedHash = pbkdf2.GetBytes(hashSize);

                return CryptographicOperations.FixedTimeEquals(hashBytes, computedHash);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Generate a cryptographically secure random password
        /// </summary>
        /// <param name="length">Password length (8-128 characters)</param>
        /// <param name="includeUppercase">Include uppercase letters</param>
        /// <param name="includeLowercase">Include lowercase letters</param>
        /// <param name="includeNumbers">Include numbers</param>
        /// <param name="includeSpecialChars">Include special characters</param>
        /// <returns>Generated password</returns>
        public static string GenerateRandomPassword(
            int length = 12,
            bool includeUppercase = true,
            bool includeLowercase = true,
            bool includeNumbers = true,
            bool includeSpecialChars = true)
        {
            if (length < MinPasswordLength || length > MaxPasswordLength)
                throw new ArgumentException($"Password length must be between {MinPasswordLength} and {MaxPasswordLength}", nameof(length));

            var charsets = new List<string>();
            var mandatoryChars = new List<char>();

            if (includeUppercase)
            {
                charsets.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                mandatoryChars.Add(GetRandomCharFromString("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            }

            if (includeLowercase)
            {
                charsets.Add("abcdefghijklmnopqrstuvwxyz");
                mandatoryChars.Add(GetRandomCharFromString("abcdefghijklmnopqrstuvwxyz"));
            }

            if (includeNumbers)
            {
                charsets.Add("0123456789");
                mandatoryChars.Add(GetRandomCharFromString("0123456789"));
            }

            if (includeSpecialChars)
            {
                charsets.Add("!@#$%^&*()_+-=[]{}|;:,.<>?");
                mandatoryChars.Add(GetRandomCharFromString("!@#$%^&*()_+-=[]{}|;:,.<>?"));
            }

            if (!charsets.Any())
                throw new ArgumentException("At least one character type must be included");

            var allChars = string.Join("", charsets);
            var password = new char[length];

            // Fill mandatory positions first
            var mandatoryPositions = new HashSet<int>();
            for (int i = 0; i < mandatoryChars.Count && i < length; i++)
            {
                int position;
                do
                {
                    position = RandomNumberGenerator.GetInt32(0, length);
                } while (mandatoryPositions.Contains(position));

                mandatoryPositions.Add(position);
                password[position] = mandatoryChars[i];
            }

            // Fill remaining positions
            for (int i = 0; i < length; i++)
            {
                if (!mandatoryPositions.Contains(i))
                {
                    password[i] = GetRandomCharFromString(allChars);
                }
            }

            return new string(password);
        }

        /// <summary>
        /// Generate a memorable password using word patterns
        /// </summary>
        /// <param name="wordCount">Number of words (2-6)</param>
        /// <param name="includeNumbers">Include numbers between words</param>
        /// <param name="includeSpecialChars">Include special characters</param>
        /// <param name="capitalizeWords">Capitalize first letter of each word</param>
        /// <returns>Generated memorable password</returns>
        public static string GenerateMemorablePassword(
            int wordCount = 3,
            bool includeNumbers = true,
            bool includeSpecialChars = true,
            bool capitalizeWords = true)
        {
            if (wordCount < 2 || wordCount > 6)
                throw new ArgumentException("Word count must be between 2 and 6", nameof(wordCount));

            var commonWords = new[]
            {
                "apple", "beach", "chair", "dance", "eagle", "flame", "grape", "house",
                "island", "jungle", "knife", "lemon", "mouse", "night", "ocean", "piano",
                "queen", "river", "stone", "table", "umbrella", "voice", "water", "zebra",
                "garden", "bridge", "castle", "dragon", "forest", "golden", "hammer", "kitten",
                "ladder", "market", "number", "orange", "pencil", "rocket", "silver", "tunnel"
            };

            var words = new List<string>();
            var usedWords = new HashSet<string>();

            for (int i = 0; i < wordCount; i++)
            {
                string word;
                do
                {
                    word = commonWords[RandomNumberGenerator.GetInt32(0, commonWords.Length)];
                } while (usedWords.Contains(word));

                usedWords.Add(word);
                
                if (capitalizeWords)
                {
                    word = char.ToUpper(word[0]) + word.Substring(1);
                }

                words.Add(word);
            }

            var password = new StringBuilder();
            
            for (int i = 0; i < words.Count; i++)
            {
                password.Append(words[i]);
                
                if (i < words.Count - 1)
                {
                    if (includeNumbers && RandomNumberGenerator.GetInt32(0, 2) == 0)
                    {
                        password.Append(RandomNumberGenerator.GetInt32(0, 100));
                    }
                    
                    if (includeSpecialChars && RandomNumberGenerator.GetInt32(0, 3) == 0)
                    {
                        var specialChars = "!@#$%^&*";
                        password.Append(specialChars[RandomNumberGenerator.GetInt32(0, specialChars.Length)]);
                    }
                }
            }

            // Ensure minimum length
            if (password.Length < MinPasswordLength)
            {
                password.Append(RandomNumberGenerator.GetInt32(10, 100));
            }

            return password.ToString();
        }

        /// <summary>
        /// Validate password strength and return detailed analysis
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Password strength result</returns>
        public static PasswordStrengthResult ValidatePasswordStrength(string password)
        {
            var result = new PasswordStrengthResult
            {
                Password = password ?? string.Empty,
                IsValid = false,
                Score = 0,
                Feedback = new List<string>()
            };

            if (string.IsNullOrEmpty(password))
            {
                result.Feedback.Add("Password is required");
                return result;
            }

            // Length check
            if (password.Length < MinPasswordLength)
            {
                result.Feedback.Add($"Password must be at least {MinPasswordLength} characters long");
            }
            else if (password.Length >= 12)
            {
                result.Score += 2;
            }
            else if (password.Length >= MinPasswordLength)
            {
                result.Score += 1;
            }

            if (password.Length > MaxPasswordLength)
            {
                result.Feedback.Add($"Password cannot exceed {MaxPasswordLength} characters");
                return result;
            }

            // Character type checks
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasSpecial = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]");

            var characterTypes = 0;
            if (hasUpper) { characterTypes++; result.Score++; }
            if (hasLower) { characterTypes++; result.Score++; }
            if (hasDigit) { characterTypes++; result.Score++; }
            if (hasSpecial) { characterTypes++; result.Score += 2; }

            if (characterTypes < 3)
            {
                result.Feedback.Add("Password should contain at least 3 of the following: uppercase letters, lowercase letters, numbers, special characters");
            }

            // Common pattern checks
            if (Regex.IsMatch(password, @"(.)\1{2,}"))
            {
                result.Feedback.Add("Avoid repeating the same character more than twice");
                result.Score--;
            }

            if (Regex.IsMatch(password, @"(012|123|234|345|456|567|678|789|890|abc|bcd|cde|def|efg|fgh|ghi|hij|ijk|jkl|klm|lmn|mno|nop|opq|pqr|qrs|rst|stu|tuv|uvw|vwx|wxy|xyz)", RegexOptions.IgnoreCase))
            {
                result.Feedback.Add("Avoid sequential characters (e.g., 123, abc)");
                result.Score--;
            }

            // Common passwords check
            var commonPasswords = new[]
            {
                "password", "123456", "password123", "admin", "letmein", "welcome",
                "monkey", "1234567890", "qwerty", "abc123", "Password1", "123456789"
            };

            if (commonPasswords.Any(cp => string.Equals(cp, password, StringComparison.OrdinalIgnoreCase)))
            {
                result.Feedback.Add("This is a commonly used password. Please choose something more unique");
                result.Score = 0;
            }

            // Determine strength level
            result.Strength = result.Score switch
            {
                >= 7 => PasswordStrength.VeryStrong,
                >= 5 => PasswordStrength.Strong,
                >= 3 => PasswordStrength.Medium,
                >= 1 => PasswordStrength.Weak,
                _ => PasswordStrength.VeryWeak
            };

            result.IsValid = result.Score >= 3 && result.Feedback.Count == 0;

            if (result.IsValid && result.Feedback.Count == 0)
            {
                result.Feedback.Add("Password meets security requirements");
            }

            return result;
        }

        /// <summary>
        /// Generate a cryptographically secure token
        /// </summary>
        /// <param name="length">Token length in bytes (default: 32)</param>
        /// <returns>Base64 encoded secure token</returns>
        public static string GenerateSecureToken(int length = 32)
        {
            if (length <= 0 || length > 256)
                throw new ArgumentException("Token length must be between 1 and 256 bytes", nameof(length));

            var tokenBytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            
            return Convert.ToBase64String(tokenBytes);
        }

        /// <summary>
        /// Generate a secure salt for password hashing
        /// </summary>
        /// <param name="size">Salt size in bytes</param>
        /// <returns>Salt bytes</returns>
        public static byte[] GenerateSecureSalt(int size = DefaultSaltSize)
        {
            if (size <= 0 || size > 64)
                throw new ArgumentException("Salt size must be between 1 and 64 bytes", nameof(size));

            var salt = new byte[size];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            
            return salt;
        }

        /// <summary>
        /// Generate a PIN with specified length
        /// </summary>
        /// <param name="length">PIN length (4-12 digits)</param>
        /// <returns>Generated PIN</returns>
        public static string GenerateSecurePIN(int length = 6)
        {
            if (length < 4 || length > 12)
                throw new ArgumentException("PIN length must be between 4 and 12 digits", nameof(length));

            var pin = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                pin.Append(RandomNumberGenerator.GetInt32(0, 10));
            }

            return pin.ToString();
        }

        /// <summary>
        /// Check if password has been compromised (basic patterns check)
        /// This is a simplified version - in production, you might use services like HaveIBeenPwned API
        /// </summary>
        /// <param name="password">Password to check</param>
        /// <returns>True if password appears to be compromised</returns>
        public static bool IsPasswordCompromised(string password)
        {
            if (string.IsNullOrEmpty(password))
                return true;

            var result = ValidatePasswordStrength(password);
            return result.Strength <= PasswordStrength.Weak;
        }

        #region Private Helper Methods

        private static char GetRandomCharFromString(string chars)
        {
            return chars[RandomNumberGenerator.GetInt32(0, chars.Length)];
        }

        #endregion
    }

    #region Supporting Classes and Enums

    /// <summary>
    /// Password strength levels
    /// </summary>
    public enum PasswordStrength
    {
        VeryWeak = 0,
        Weak = 1,
        Medium = 2,
        Strong = 3,
        VeryStrong = 4
    }

    /// <summary>
    /// Password strength validation result
    /// </summary>
    public class PasswordStrengthResult
    {
        public string Password { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public int Score { get; set; }
        public PasswordStrength Strength { get; set; }
        public List<string> Feedback { get; set; } = new List<string>();
        
        public string StrengthDescription => Strength switch
        {
            PasswordStrength.VeryWeak => "Very Weak - This password is easily guessable",
            PasswordStrength.Weak => "Weak - This password could be guessed",
            PasswordStrength.Medium => "Medium - This password is okay but could be stronger",
            PasswordStrength.Strong => "Strong - This password is secure",
            PasswordStrength.VeryStrong => "Very Strong - This password is very secure",
            _ => "Unknown"
        };
    }

    #endregion
}