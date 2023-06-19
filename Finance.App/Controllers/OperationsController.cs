using AutoMapper;
using Finance.App.Domain.Models;
using Finance.App.Domain.Services;
using Finance.App.Dtos;
using Finance.App.Helpers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finance.App.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OperationsController : Controller
    {
        private readonly IOperationService _operationService;
        private readonly IMapper _mapper;
        private readonly ILogger<OperationsController> _logger;

        public OperationsController(IOperationService operationService, IMapper mapper, ILogger<OperationsController> logger)
        {
            _operationService = operationService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FinancialOperationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all financial operations...");

                var operations = await _operationService.ListAsync();
                var operationDtos = _mapper.Map<IEnumerable<FinancialOperationDto>>(operations);

                _logger.LogInformation($"Successfully fetched {operationDtos.Count()} financial operations.");

                return Ok(operationDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all financial operations.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while fetching all financial operations.");
            }
        }

        [HttpGet("{id}", Name = "GetOperationById")]
        [ProducesResponseType(typeof(FinancialOperationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching financial operation with ID {id}...");

                var operation = await _operationService.GetByIdAsync(id);

                if (operation == null)
                {
                    _logger.LogWarning($"Financial operation with ID {id} not found.");
                    return NotFound($"Financial operation with ID {id} not found.");
                }

                var operationDto = _mapper.Map<FinancialOperationDto>(operation);
                _logger.LogInformation($"Successfully fetched financial operation with ID {id}.");
                return Ok(operationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching financial operation with ID {id}.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while fetching financial operation with ID {id}.");
            }
        }

        [HttpGet("date/daily/{date}")]
        [ProducesResponseType(typeof(OperationsByDateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailyAsync(string date)
        {
            try
            {
                _logger.LogInformation($"Fetching financial operations for date {date}...");

                var operations = await _operationService.ListByDateAsync(date, null);
                var mappedOperations = _mapper.Map<IEnumerable<FinancialOperationDto>>(operations.Operations);

                if (!mappedOperations.Any())
                {
                    return NoContent();
                }

                var operationsByDateDto = _mapper.Map<OperationsByDateDto>(operations);
                operationsByDateDto.Operations = mappedOperations;

                _logger.LogInformation($"Successfully fetched {mappedOperations.Count()} " +
                    $"financial operations for date {date}.");

                return Ok(operationsByDateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching financial operations for date {date}.");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while fetching financial operations for date {date}.");
            }
        }

        [HttpGet("date/range/{startDate}/{endDate}")]
        [ProducesResponseType(typeof(OperationsByDateDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByDateRangeAsync(string startDate, string endDate)
        {
            try
            {
                _logger.LogInformation($"Fetching financial operations for date range {startDate} to {endDate}...");

                var operations = await _operationService.ListByDateAsync(startDate, endDate);
                var mappedOperations = _mapper.Map<IEnumerable<FinancialOperationDto>>(operations.Operations);

                if (!mappedOperations.Any())
                {
                    return NoContent();
                }

                var operationsByDateDto = _mapper.Map<OperationsByDateDto>(operations);
                operationsByDateDto.Operations = mappedOperations;

                _logger.LogInformation($"Successfully fetched {mappedOperations.Count()} " +
                    $"financial operations for date range {startDate} to {endDate}.");

                return Ok(operationsByDateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching financial operations " +
                    $"for date range {startDate} to {endDate}.");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An error occurred while fetching financial operations for date range {startDate} to {endDate}.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(FinancialOperationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostAsync([FromBody] SaveFinancialOperationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogError("Invalid model state.");
                    return BadRequest(ModelState.GetErrorMessages());
                }

                _logger.LogInformation("Creating new financial operation.");

                var operation = _mapper.Map<SaveFinancialOperationDto, FinancialOperation>(dto);
                var result = await _operationService.SaveAsync(operation);

                if (!result.Success)
                {
                    _logger.LogError($"Error creating new financial operation: ", result.Message);
                    return BadRequest(result.Message);
                }

                _logger.LogInformation($"New financial operation created with ID: ", result.Operation.Id);

                var operationDto = _mapper.Map<FinancialOperation, FinancialOperationDto>(operation);
                return CreatedAtRoute("GetOperationById", new { id = operationDto.Id }, operationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new financial operation.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while creating a new financial operation.");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(FinancialOperationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutAsync(int id, [FromBody] SaveFinancialOperationDto dto)
        {
            try
            {
                _logger.LogInformation($"Updating financial operation with ID {0}", id);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid model state while updating financial operation with ID {0}", id);
                    return BadRequest(ModelState.GetErrorMessages());
                }

                var operation = _mapper.Map<SaveFinancialOperationDto, FinancialOperation>(dto);
                var result = await _operationService.UpdateAsync(id, operation);

                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result.Message);
                    }

                    _logger.LogWarning($"Failed to update financial operation with ID {0}: {1}", id, result.Message);
                    return BadRequest(result.Message);
                }

                var operationDto = _mapper.Map<FinancialOperation, FinancialOperationDto>(result.Operation);
                _logger.LogInformation($"Successfully updated financial operation with ID {0}", id);
                return Ok(operationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating financial operation with ID {0}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while updating the financial operation.");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(FinancialOperationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting operation with id {id}");
                var result = await _operationService.DeleteAsync(id);
                if (!result.Success)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return NotFound(result.Message);
                    }

                    _logger.LogWarning($"Failed to delete operation with id {id}: {result.Message}");
                    return BadRequest(result.Message);
                }

                var operationDto = _mapper.Map<FinancialOperation, FinancialOperationDto>(result.Operation);
                _logger.LogInformation($"Operation with id {id} deleted successfully");
                return Ok(operationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting operation with id {id}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while deleting the operation");
            }
        }
    }
}
