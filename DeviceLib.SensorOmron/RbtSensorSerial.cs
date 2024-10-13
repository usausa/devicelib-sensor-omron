namespace DeviceLib.SensorOmron;

using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Ports;

public sealed class RbtSensorSerial : IDisposable
{
    private static readonly byte[] MeasureCommand = MakeCommand([0x01, 0x21, 0x50]);

    private readonly SerialPort port;

    private readonly byte[] buffer;

    private bool disposed;

    public float? Temperature { get; private set; }

    public float? Humidity { get; private set; }

    public float? Light { get; private set; }

    public float? Pressure { get; private set; }

    public float? Noise { get; private set; }

    public float? Discomfort { get; private set; }

    public float? Heat { get; private set; }

    public float? Etvoc { get; private set; }

    public float? Eco2 { get; private set; }

    public float? Seismic { get; private set; }

    public RbtSensorSerial(string name)
    {
        port = new SerialPort(name)
        {
            BaudRate = 115200,
            DataBits = 8,
            StopBits = StopBits.One,
            Parity = Parity.None,
            Handshake = Handshake.None,
            ReadTimeout = 5000
        };

        buffer = ArrayPool<byte>.Shared.Rent(256);
    }

    public void Dispose()
    {
        if (!disposed)
        {
            ArrayPool<byte>.Shared.Return(buffer);

            port.Dispose();

            disposed = true;
        }
    }

    public bool IsOpen() => port.IsOpen;

    public void Open()
    {
        if (IsOpen())
        {
            return;
        }

        port.Open();
        port.DiscardOutBuffer();
        port.DiscardInBuffer();
    }

    public void Close()
    {
        port.Close();
    }

    private void ClearValues()
    {
        Temperature = null;
        Humidity = null;
        Light = null;
        Pressure = null;
        Noise = null;
        Discomfort = null;
        Heat = null;
        Etvoc = null;
        Eco2 = null;
        Seismic = null;
    }

    public async ValueTask<bool> UpdateAsync(CancellationToken cancel = default)
    {
        ClearValues();

        var read = await TransitAsync(MeasureCommand, cancel).ConfigureAwait(false);
        if (read < 35)
        {
            return false;
        }

        Temperature = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(8, 2)) / 100;
        Humidity = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(10, 2)) / 100;
        Light = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(12, 2));
        Pressure = (float)BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(14, 4)) / 1000;
        Noise = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(18, 2)) / 100;

        Discomfort = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(24, 2)) / 100;
        Heat = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(26, 2)) / 100;

        Etvoc = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(20, 2));
        Eco2 = BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(22, 2));

        Seismic = (float)BinaryPrimitives.ReadInt16LittleEndian(buffer.AsSpan(33, 2)) / 1000;

        return true;
    }

    public ValueTask<bool> LedOnAsync(byte r, byte g, byte b, CancellationToken cancel = default) =>
        LedAsync(0x01, r, g, b, cancel);

    public ValueTask<bool> LedOffAsync(CancellationToken cancel = default) =>
        LedAsync(0x00, 0x00, 0x00, 0x00, cancel);

    private async ValueTask<bool> LedAsync(byte on, byte r, byte g, byte b, CancellationToken cancel = default)
    {
        var size = 8 + 6;
        var command = ArrayPool<byte>.Shared.Rent(size);
        try
        {
            MakeCommand(command.AsSpan(0, size), [0x02, 0x11, 0x51, on, 0x00, r, g, b]);
            var length = await TransitAsync(command.AsMemory(0, size), cancel).ConfigureAwait(false);
            return length > 0;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(command);
        }
    }

    private async ValueTask<int> TransitAsync(ReadOnlyMemory<byte> command, CancellationToken cancel = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        if (!IsOpen())
        {
            return 0;
        }

        await port.BaseStream.WriteAsync(command, cancel).ConfigureAwait(false);
        var read = 0;
        var length = 4;
        while (true)
        {
#pragma warning disable CA1835
            read += await port.BaseStream.ReadAsync(buffer, read, length - read, cancel).ConfigureAwait(false);
#pragma warning restore CA1835
            if (read == length)
            {
                if (read == 4)
                {
                    length += BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(2, 2));
                }
                else
                {
                    break;
                }
            }
        }

        return read;
    }

    public static byte[] MakeCommand(Span<byte> payload)
    {
        var command = new byte[payload.Length + 6];
        MakeCommand(command, payload);
        return command;
    }

    public static void MakeCommand(Span<byte> buffer, Span<byte> payload)
    {
        buffer[0] = 0x52;
        buffer[1] = 0x42;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(2, 2), (ushort)(payload.Length + 2));
        payload.CopyTo(buffer[4..]);
        var crc = CalcCrc(buffer[..^2]);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(buffer.Length - 2, 2), crc);
    }

    private static ushort CalcCrc(Span<byte> span)
    {
        var crc = (ushort)0xFFFF;
        for (var i = 0; i < span.Length; i++)
        {
            crc = (ushort)(crc ^ span[i]);
            for (var j = 0; j < 8; j++)
            {
                var carry = crc & 1;
                if (carry != 0)
                {
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                }
                else
                {
                    crc >>= 1;
                }
            }
        }

        return crc;
    }
}
