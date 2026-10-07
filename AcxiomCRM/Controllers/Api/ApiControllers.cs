using AcxiomCRM.DTOs;
using AcxiomCRM.Models.Identity;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels.Customer;
using AcxiomCRM.ViewModels.Opportunity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// ═══════════════════════════════════════════════════════════════════════════════
// CUSTOMERS API
// ═══════════════════════════════════════════════════════════════════════════════
namespace AcxiomCRM.Controllers.Api
{
    [Route("api/customers")]
    [ApiController]
    [Authorize]
    public class CustomersApiController : ControllerBase
    {
        private readonly ICustomerService              _svc;
        private readonly IAuditService                 _audit;
        private readonly UserManager<ApplicationUser> _users;

        public CustomersApiController(ICustomerService svc, IAuditService audit,
            UserManager<ApplicationUser> users)
        {
            _svc   = svc;
            _audit = audit;
            _users = users;
        }

        // GET /api/customers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user  = await _users.GetUserAsync(User);
            var roles = await _users.GetRolesAsync(user!);
            bool isPriv = roles.Contains("Admin") || roles.Contains("Manager");

            var customers = await _svc.GetAllAsync(user!.Id, isPriv);
            var dtos = customers.Select(c => new CustomerDto
            {
                CustomerId   = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Email        = c.Email,
                Phone        = c.Phone,
                CompanyName  = c.CompanyName,
                Status       = c.Status,
                CreatedDate  = c.CreatedDate
            });
            return Ok(dtos);
        }

        // GET /api/customers/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var c = await _svc.GetByIdAsync(id);
            if (c == null)
                return NotFound(new ApiError { Message = "Customer not found." });

            return Ok(new CustomerDto
            {
                CustomerId   = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Email        = c.Email,
                Phone        = c.Phone,
                CompanyName  = c.CompanyName,
                Status       = c.Status,
                CreatedDate  = c.CreatedDate
            });
        }

        // POST /api/customers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _svc.EmailExistsAsync(dto.Email))
                return Conflict(new ApiError { Message = "A customer with this email already exists.", Field = "email" });

            if (await _svc.PhoneExistsAsync(dto.Phone))
                return Conflict(new ApiError { Message = "A customer with this phone already exists.", Field = "phone" });

            var user = await _users.GetUserAsync(User);
            var vm   = new CustomerViewModel
            {
                CustomerName = dto.CustomerName,
                Email        = dto.Email,
                Phone        = dto.Phone,
                CompanyName  = dto.CompanyName,
                Status       = dto.Status
            };

            var c  = await _svc.CreateAsync(vm, user!.Id);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _audit.LogAsync(user.Id, user.Email ?? "", "Create", "Customer",
                c.CustomerId.ToString(), ipAddress: ip);

            return CreatedAtAction(nameof(GetById), new { id = c.CustomerId },
                new CustomerDto
                {
                    CustomerId   = c.CustomerId,
                    CustomerCode = c.CustomerCode,
                    CustomerName = c.CustomerName,
                    Email        = c.Email,
                    Phone        = c.Phone,
                    Status       = c.Status
                });
        }

        // PUT /api/customers/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCustomerDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _svc.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiError { Message = "Customer not found." });

            if (await _svc.EmailExistsAsync(dto.Email, id))
                return Conflict(new ApiError { Message = "Email already in use.", Field = "email" });

            if (await _svc.PhoneExistsAsync(dto.Phone, id))
                return Conflict(new ApiError { Message = "Phone already in use.", Field = "phone" });

            var user = await _users.GetUserAsync(User);
            var vm   = new CustomerViewModel
            {
                CustomerId   = id,
                CustomerName = dto.CustomerName,
                Email        = dto.Email,
                Phone        = dto.Phone,
                CompanyName  = dto.CompanyName,
                Status       = dto.Status,
                AssignedTo   = existing.AssignedTo
            };

            await _svc.UpdateAsync(vm);
            await _audit.LogAsync(user!.Id, user.Email ?? "", "Update", "Customer",
                id.ToString(), ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Customer updated successfully." });
        }

        // DELETE /api/customers/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _svc.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new ApiError { Message = "Customer not found." });

            await _svc.DeactivateAsync(id);
            var user = await _users.GetUserAsync(User);
            await _audit.LogAsync(user!.Id, user.Email ?? "", "Deactivate", "Customer",
                id.ToString(), ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = "Customer deactivated." });
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// LEADS API
// ═══════════════════════════════════════════════════════════════════════════════
namespace AcxiomCRM.Controllers.Api
{
    [Route("api/leads")]
    [ApiController]
    [Authorize]
    public class LeadsApiController : ControllerBase
    {
        private readonly ILeadService                  _svc;
        private readonly IAuditService                 _audit;
        private readonly UserManager<ApplicationUser> _users;

        public LeadsApiController(ILeadService svc, IAuditService audit,
            UserManager<ApplicationUser> users)
        {
            _svc   = svc;
            _audit = audit;
            _users = users;
        }

        // GET /api/leads
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user  = await _users.GetUserAsync(User);
            var roles = await _users.GetRolesAsync(user!);
            bool isPriv = roles.Contains("Admin") || roles.Contains("Manager");

            var leads = await _svc.GetAllAsync(user!.Id, isPriv);
            return Ok(leads.Select(l => new LeadDto
            {
                LeadId        = l.LeadId,
                LeadCode      = l.LeadCode,
                LeadName      = l.LeadName,
                Email         = l.Email,
                Phone         = l.Phone,
                Source        = l.Source,
                Status        = l.Status,
                Priority      = l.Priority,
                ExpectedValue = l.ExpectedValue,
                CreatedDate   = l.CreatedDate
            }));
        }

        // POST /api/leads
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] LeadDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var validStatuses = new[] { "New", "Contacted", "Qualified", "Unqualified", "Converted", "Lost" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new ApiError { Message = "Invalid lead status.", Field = "status" });

            var user = await _users.GetUserAsync(User);
            var vm   = new AcxiomCRM.ViewModels.Lead.LeadViewModel
            {
                LeadName      = dto.LeadName,
                Email         = dto.Email,
                Phone         = dto.Phone,
                Source        = dto.Source,
                Status        = dto.Status,
                Priority      = dto.Priority,
                ExpectedValue = dto.ExpectedValue
            };

            var lead = await _svc.CreateAsync(vm, user!.Id);
            var ip   = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _audit.LogAsync(user.Id, user.Email ?? "", "Create", "Lead",
                lead.LeadId.ToString(), ipAddress: ip);

            dto.LeadId    = lead.LeadId;
            dto.LeadCode  = lead.LeadCode;
            dto.CreatedDate = lead.CreatedDate;

            return Created($"/api/leads/{lead.LeadId}", dto);
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// OPPORTUNITIES API
// ═══════════════════════════════════════════════════════════════════════════════
namespace AcxiomCRM.Controllers.Api
{
    [Route("api/opportunities")]
    [ApiController]
    [Authorize]
    public class OpportunitiesApiController : ControllerBase
    {
        private readonly IOpportunityService           _svc;
        private readonly IAuditService                 _audit;
        private readonly UserManager<ApplicationUser> _users;

        public OpportunitiesApiController(IOpportunityService svc, IAuditService audit,
            UserManager<ApplicationUser> users)
        {
            _svc   = svc;
            _audit = audit;
            _users = users;
        }

        // GET /api/opportunities
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user  = await _users.GetUserAsync(User);
            var roles = await _users.GetRolesAsync(user!);
            bool isPriv = roles.Contains("Admin") || roles.Contains("Manager");

            var opps = await _svc.GetAllAsync(user!.Id, isPriv);
            return Ok(opps.Select(o => new OpportunityDto
            {
                OpportunityId    = o.OpportunityId,
                OpportunityName  = o.OpportunityName,
                CustomerId       = o.CustomerId,
                Stage            = o.Stage,
                Amount           = o.Amount,
                Probability      = o.Probability,
                ExpectedCloseDate = o.ExpectedCloseDate,
                Status           = o.Status,
                WeightedAmount   = o.WeightedAmount
            }));
        }

        // POST /api/opportunities
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OpportunityDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Business rules enforced at API boundary too
            if (dto.Amount <= 0)
                return BadRequest(new ApiError
                    { Message = "Opportunity Amount must be greater than 0.", Field = "amount" });

            if (dto.Probability < 0 || dto.Probability > 100)
                return BadRequest(new ApiError
                    { Message = "Probability must be between 0 and 100.", Field = "probability" });

            if (dto.ExpectedCloseDate.Date < DateTime.Today)
                return BadRequest(new ApiError
                    { Message = "Expected Close Date cannot be in the past.", Field = "expectedCloseDate" });

            var user = await _users.GetUserAsync(User);
            var vm   = new OpportunityViewModel
            {
                OpportunityName   = dto.OpportunityName,
                CustomerId        = dto.CustomerId,
                Stage             = dto.Stage,
                Amount            = dto.Amount,
                Probability       = dto.Probability,
                ExpectedCloseDate = dto.ExpectedCloseDate,
                Status            = "Open",
                AssignedTo        = user!.Id
            };

            var opp = await _svc.CreateAsync(vm, user.Id);
            var ip  = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _audit.LogAsync(user.Id, user.Email ?? "", "Create", "Opportunity",
                opp.OpportunityId.ToString(), ipAddress: ip);

            dto.OpportunityId  = opp.OpportunityId;
            dto.WeightedAmount = opp.WeightedAmount;

            return Created($"/api/opportunities/{opp.OpportunityId}", dto);
        }
    }
}
