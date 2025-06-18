



using System.Security.Cryptography;

namespace Bornlogic.IdentityServer.Services.Default
{
    /// <summary>
    /// User code generator using 9 digit number
    /// </summary>
    /// <seealso cref="IUserCodeGenerator" />
    public class NumericUserCodeGenerator : IUserCodeGenerator
    {
        /// <summary>
        /// Gets the type of the user code.
        /// </summary>
        /// <value>
        /// The type of the user code.
        /// </value>
        public string UserCodeType => IdentityServerConstants.UserCodeTypes.Numeric;

        /// <summary>
        /// Gets the retry limit.
        /// </summary>
        /// <value>
        /// The retry limit for getting a unique value.
        /// </value>
        public int RetryLimit => 5;

        /// <summary>
        /// Generates the user code.
        /// </summary>
        /// <returns></returns>
        public Task<string> GenerateAsync()
        {
            var next = Next(100000000, 999999999);
            return Task.FromResult(next.ToString());
        }

        private int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue) throw new ArgumentOutOfRangeException(nameof(minValue));
            if (minValue == maxValue) return minValue;

            return RandomNumberGenerator.GetInt32(minValue, maxValue);
        }
    }
}