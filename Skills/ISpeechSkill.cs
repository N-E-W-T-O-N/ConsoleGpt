namespace console_gpt.Skills
{
    /// <summary>
    /// An interface for a Sematic Kernel skill that provides the ability to read and write from the console
    /// </summary>
    public interface ISpeechSkill
    {
        /// <summary>
        /// Gets input from the user
        /// </summary>
        public Task<string> Listen();

        /// <summary>
        /// Responds tp the user
        /// </summary>
        public Task<string> Respond(string message);//, SKContext context);


        /// <summary>
        /// Gets if Listen function detected goodbye from the user
        /// </summary>
        public Task<string> IsGoodbye();//(SKContext context);
    }
}