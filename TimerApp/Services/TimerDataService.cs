using Microsoft.EntityFrameworkCore;

namespace TimerApp.Services
{
    public class TimerDataService
    {
        private readonly TimerContext _context;
        public TimerDataService(TimerContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task AddDataAsync(Data.Timer timer, CancellationToken cancellationToken = default)
        {

            if (timer == null)
                throw new ArgumentNullException(nameof(timer));
            try
            {
                await _context.Timer.AddAsync(timer, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while adding the timer data.", ex);
            }
        }

        public async Task<IEnumerable<Data.Timer>> GetDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Timer.AsNoTracking().ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving the timer data.", ex);
            }
        }
    }
}
