namespace AI_ColdCall_Agent.Core.DTO
{
    public class CallLogFilterDto
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > 50 ? 50 : value);
        }

        public CallLogOutcome? CallOutcomeId { get; set; }
        public int? CallSessionStateId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchTerm { get; set; }
    }

    public enum CallLogOutcome
    {
        Interested=1,
        NotInterested=2,
        NotAnswer=3,
        Failed= 4
	}
}
