using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TraceView.Listener
{
  public class TraceViewListener : TraceListener
  {

    private UdpClient _client = null;
    private ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();
    private CancellationTokenSource _cancellation = new CancellationTokenSource();
    private Task _work = null;
    
    /// <summary>
    /// Creates a TraceView listener
    /// </summary>
    /// <param name="hostname">Hostname of the sever/host running TraceView. Default is localhost.</param>
    /// <param name="port">UDP port that the host is listening on. Default is 1969 (the best year in music)</param>
    public TraceViewListener(string hostname = "localhost", int port = 1969)
    {
      _client = new UdpClient(hostname, port);
      _work = Task.Run(async () =>
      {
        while (!_cancellation.IsCancellationRequested)
        {
          string s = null;
          if (_queue.TryDequeue(out s))
          {
            try
            {
              var buffer = Encoding.UTF8.GetBytes(s);
              await _client.SendAsync(buffer, buffer.Length);
            }
            catch (Exception)
            {
              // Tough luck!
              // Remember that joke about UDP? 
              // Not saying as you may not get it.
            }
          }
          else
          {
            await Task.Delay(TimeSpan.FromMilliseconds(200), _cancellation.Token);
          }
        }
      });
    }

    public override void Write(string message)
    {
      WriteLine(message);
    }

    protected override void Dispose(bool disposing)
    {
      try
      {
        base.Dispose(disposing);
        _cancellation.Cancel();
        _work.ConfigureAwait(false).GetAwaiter().GetResult();
      }
      catch (Exception)
      {
        // dev/null
      }
    }

    public override void WriteLine(string message)
    {
      _queue.Enqueue(message);
    }
  }
}