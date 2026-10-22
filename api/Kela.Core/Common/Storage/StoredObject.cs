namespace Kela.Core.Common.Storage;

/// <summary>Bucket-dakı bir obyekt: açar, ölçü və son dəyişiklik vaxtı (storage ölçməsi üçün).</summary>
public sealed record StoredObject(string Key, long SizeBytes, DateTimeOffset LastModified);
