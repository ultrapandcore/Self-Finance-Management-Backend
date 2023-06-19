using Finance.App.Domain.Models;

namespace Finance.App.Domain.Services.Communication
{
    public class OperationResponse : BaseResponse
    {
        public FinancialOperation Operation { get; set; }
        public OperationResponse(bool success, string message, FinancialOperation operation) : base(success, message)
        {
            Operation = operation;
        }

        /// <summary>
        /// Creates a success response.
        /// </summary>
        /// <param name="operation">Saved operation.</param>
        /// <returns>Response.</returns>
        public OperationResponse(FinancialOperation operation) : this(true, string.Empty, operation)
        { }

        /// <summary>
        /// Creates am error response.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <returns>Response.</returns>
        public OperationResponse(string message) : this(false, message, null)
        { }
    }
}
