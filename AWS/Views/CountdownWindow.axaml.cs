using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AWS;
public partial class CountdownWindow : Window
{
    private TimeSpan _remainingTime;
    private CancellationTokenSource _cts;
    private bool _isPaused = false;
    
    // ��������� �������� ��� ������� � ����������� �������
    public TimeSpan RemainingTime => _remainingTime;
    public bool WasCancelled { get; private set; } = false;
    
    public CountdownWindow(TimeSpan initialTime)
    {
        InitializeComponent();
        _remainingTime = initialTime;
        UpdateTimerDisplay();
        
        // ��������� ������ ��� �������� ����
        StartCountdown();
    }
    
    private void StartCountdown()
    {
        _cts = new CancellationTokenSource();
        
        Task.Run(async () =>
        {
            while (_remainingTime > TimeSpan.Zero && !_cts.Token.IsCancellationRequested)
            {
                if (!_isPaused)
                {
                    await Task.Delay(1000, _cts.Token); // ���� 1 �������
                    
                    if (!_cts.Token.IsCancellationRequested && !_isPaused)
                    {
                        _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
                        
                        // ��������� UI � ������� ������
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            UpdateTimerDisplay();
                        });
                    }
                }
                else
                {
                    await Task.Delay(100); // �������� ����� ��� ���������
                }
            }
            
            // ����� ����� �����
            if (_remainingTime <= TimeSpan.Zero && !WasCancelled)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Close(true); // ������������ �� ����������
                });
            }
        }, _cts.Token);
    }
    
    private void UpdateTimerDisplay()
    {
        TimerText.Text = _remainingTime.ToString(@"mm\:ss");
        
        // ������ ���� � ����������� �� ����������� �������
        if (_remainingTime.TotalMinutes < 1)
        {
            TimerText.Foreground = Avalonia.Media.Brushes.Red;
        }
        else if (_remainingTime.TotalMinutes < 3)
        {
            TimerText.Foreground = Avalonia.Media.Brushes.Orange;
        }
        else
        {
            TimerText.Foreground = Avalonia.Media.Brushes.Green;
        }
    }
    
    private void PauseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _isPaused = !_isPaused;
        PauseButton.Content = _isPaused ? "����������" : "�����";
    }
    
    private void CancelButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WasCancelled = true;
        _cts?.Cancel();
        Close(false);
    }
    
    protected override void OnClosed(EventArgs e)
    {
        _cts?.Cancel();
        base.OnClosed(e);
    }
}