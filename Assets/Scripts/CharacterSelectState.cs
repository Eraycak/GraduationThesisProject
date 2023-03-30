using System;
using Unity.Netcode;

public struct CharacterSelectState : INetworkSerializable, IEquatable<CharacterSelectState>
{
    public ulong ClientId;
    public int CharacterId;
    public bool IsReady;

    public CharacterSelectState(ulong clientId, int characterId = -1, bool isReady = false)
    {
        ClientId = clientId;
        CharacterId = characterId;
        IsReady = isReady;
    }

    public bool Equals(CharacterSelectState other)
    {
        return ClientId == other.ClientId && CharacterId == other.CharacterId && IsReady == other.IsReady; 
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref CharacterId);
        serializer.SerializeValue(ref IsReady);
    }
}
