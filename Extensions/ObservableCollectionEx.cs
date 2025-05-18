using System.Collections.ObjectModel;
using System.Linq;
using DynamicData;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Extensions;

public static class ObservableCollectionEx
{
    public static void Sort(this ObservableCollection<PreviewImage> list)
    {
        var arr = list.OrderBy(x => x.Id).ToArray();
        list.Clear();
        list.AddRange(arr);
    }
}
