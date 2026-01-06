using Odin.System;

namespace Odin.Email
{
    /// <summary>
    /// Minimalistic IEmailSender that does nothing.
    /// </summary>
    public sealed class NullEmailSender : IEmailSender
    {
        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<ResultValue<string>> SendEmail(IEmailMessage email)
        {
            return await Task.FromResult(ResultValue<string>.Success("12345"));
        }
    }
}
