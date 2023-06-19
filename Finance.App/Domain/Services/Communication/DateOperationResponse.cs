using Finance.App.Domain.Models;

namespace Finance.App.Domain.Services.Communication
{
    public class DateOperationResponse : BaseResponse
    {
        public IEnumerable<FinancialOperation> Operations { get; set; }
        public decimal Income { get; set; } = 0;
        public decimal Expenses { get; set; } = 0;

        public DateOperationResponse(bool success, string message,
                                    IEnumerable<FinancialOperation> operations,
                                    decimal income, decimal expenses) : base(success, message)
        {
            Operations = operations;
            Income = income;
            Expenses = expenses;
        }

        /// <summary>
        /// Creates a success response.
        /// </summary>
        /// <param name="operations">Successfully retrieved operations.</param>
        /// <returns>Response.</returns>
        public DateOperationResponse(IEnumerable<FinancialOperation> operations, decimal income, decimal expenses)
                                    : this(true, string.Empty, operations, income, expenses)
        { }

        /// <summary>
        /// Creates am error response.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <returns>Response.</returns>
        public DateOperationResponse(string message) : this(false, message, null, 0, 0)
        { }
    }
}
