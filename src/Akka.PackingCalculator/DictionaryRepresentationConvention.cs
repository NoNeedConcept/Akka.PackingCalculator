using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;

namespace Akka.PackingCalculator;

public class DictionaryRepresentationConvention : ConventionBase, IMemberMapConvention
{
    private readonly DictionaryRepresentation _dictionaryRepresentation;

    public DictionaryRepresentationConvention(
        DictionaryRepresentation dictionaryRepresentation = DictionaryRepresentation.ArrayOfDocuments)
    {
        // see http://mongodb.github.io/mongo-csharp-driver/2.2/reference/bson/mapping/#dictionary-serialization-options

        _dictionaryRepresentation = dictionaryRepresentation;
    }

    public void Apply(BsonMemberMap memberMap)
    {
        memberMap.SetSerializer(ConfigureSerializer(memberMap.GetSerializer(), Array.Empty<IBsonSerializer>()));
    }

    private IBsonSerializer ConfigureSerializer(IBsonSerializer serializer, IBsonSerializer[] stack)
    {
        if (serializer is IDictionaryRepresentationConfigurable dictionaryRepresentationConfigurable)
        {
            serializer = dictionaryRepresentationConfigurable.WithDictionaryRepresentation(_dictionaryRepresentation);
        }

        if (serializer is IChildSerializerConfigurable childSerializerConfigurable)
        {
            if (!stack.Contains(childSerializerConfigurable.ChildSerializer))
            {
                var newStack = stack.Union(new[] { serializer }).ToArray();
                var childConfigured = ConfigureSerializer(childSerializerConfigurable.ChildSerializer, newStack);
                return childSerializerConfigurable.WithChildSerializer(childConfigured);
            }
        }

        return serializer;
    }
}