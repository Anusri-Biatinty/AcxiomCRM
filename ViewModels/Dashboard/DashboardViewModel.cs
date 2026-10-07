namespace AcxiomCRM.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int     TotalCustomers     { get; set; }
        public int     TotalLeads         { get; set; }
        public int     OpenLeads          { get; set; }
        public int     TotalOpportunities { get; set; }
        public int     OpenOpportunities  { get; set; }
        public int     WonOpportunities   { get; set; }
        public int     LostOpportunities  { get; set; }
        public decimal TotalPipelineValue { get; set; }
        public int     PendingFollowUps   { get; set; }
        public int     OverdueFollowUps   { get; set; }

        // JSON strings for Chart.js
        public string LeadStatusJson    { get; set; } = "{}";
        public string PipelineJson      { get; set; } = "{}";
        public string MonthlySalesJson  { get; set; } = "{}";
    }
}
