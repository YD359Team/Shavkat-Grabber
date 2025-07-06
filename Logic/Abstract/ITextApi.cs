using System.Threading.Tasks;
using Shavkat_grabber.Pattern;

namespace Shavkat_grabber.Logic.Abstract;

public interface ITextApi
{
    Task<Result<string>> GetTextResultAsync(string input);
}
