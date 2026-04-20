using CommunityToolkit.Mvvm.ComponentModel;

namespace AmThucQuan4.Native.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusText = "Sẵn sàng";

    public bool IsNotBusy => !IsBusy;
}
