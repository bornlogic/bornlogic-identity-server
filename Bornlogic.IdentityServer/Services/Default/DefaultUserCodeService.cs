namespace Bornlogic.IdentityServer.Services.Default
{
    /// <summary>
    /// Default user code service implementation.
    /// </summary>
    /// <seealso cref="IUserCodeService" />
    public class DefaultUserCodeService : IUserCodeService
    {
        private readonly IEnumerable<IUserCodeGenerator> _generators;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultUserCodeService"/> class.
        /// </summary>
        /// <param name="generators">The generators.</param>
        /// <exception cref="ArgumentNullException">generators</exception>
        public DefaultUserCodeService(IEnumerable<IUserCodeGenerator> generators)
        {
            _generators = generators ?? throw new ArgumentNullException(nameof(generators));
        }

        /// <summary>
        /// Gets the user code generator.
        /// </summary>
        /// <param name="userCodeType">Type of user code.</param>
        /// <returns></returns>
        public Task<IUserCodeGenerator> GetGenerator(string userCodeType) =>
            Task.FromResult(_generators.FirstOrDefault(x => x.UserCodeType == userCodeType));
    }
}