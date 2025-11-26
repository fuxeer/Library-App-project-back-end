using Microsoft.AspNetCore.Mvc;
using App_library_back_end.Data;
using App_library_back_end.Model;

namespace App_library_back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly ReservationService _reservationService;

        public ReservationController(ReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet("available/{bookId}")]
        public IActionResult GetAvailableRanges(int bookId, int dayAhead = 90)
        {
            try
            {
                var ranges = _reservationService.GetAvailableRanges(bookId, dayAhead);

                var result = ranges.Select(r => new
                {
                    start = r.Start.ToString("yyyy-MM-dd"),
                    end = r.End.ToString("yyyy-MM-dd")
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("add")]
        public IActionResult AddReservation([FromBody] ReservationRequest request)
        {
            try
            {
                bool success = _reservationService.AddReservation(
                    request.UserId,
                    request.BookId,
                    request.StartDate,
                    request.DueDate
                );

                if (!success)
                    return BadRequest(new { message = "Book cannot be reserved for the selected date." });

                return Ok(new { message = "Reservation added successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
