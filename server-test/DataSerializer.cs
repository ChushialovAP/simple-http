using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace servertest
{
    public class DataSerializer
    {
        public enum SerializationType
        {
            xml,
            json
        }

        static SerializationType Type { get; set; }

        public DataSerializer(string type)
        {
            Type = (SerializationType)Enum.Parse(typeof(SerializationType), type);
        }

        public Input Deserialized(string input)
        {
            Input deserialized;

            Console.WriteLine(Type.ToString());

            switch (Type)
            {
                case SerializationType.xml:

                    var serializer = new XmlSerializer(typeof(Input));

                    using (TextReader reader = new StringReader(input))
                    {
                        deserialized = (Input)serializer.Deserialize(reader);
                    }

                    break;
                case SerializationType.json:

                    deserialized = JsonConvert.DeserializeObject<Input>(input);

                    break;
                default:

                    return null;
            }

            return deserialized;
        }

        public string Serialized(Output inputObject)
        {
            string serializedToString;

            switch (Type)
            {
                case SerializationType.xml:

                    using (var stringWriter = new StringWriter())
                    {
                        var serializer = new XmlSerializer(typeof(Output));

                        serializer.Serialize(stringWriter, inputObject);
                        serializedToString = stringWriter.ToString();
                    }

                    break;
                case SerializationType.json:

                    serializedToString = JsonConvert.SerializeObject(inputObject);

                    break;
                default:

                    return null;
            }

            return serializedToString;
        }

        public Output GetAnswer(Input deserializedInput)
        { 

            Output output = new Output
            {
                SumResult = deserializedInput.Sums.Sum() * deserializedInput.K,
                MulResult = deserializedInput.Muls.Aggregate((a, b) => a * b),
                SortedInputs = TwoArraysInOne(deserializedInput.Sums, deserializedInput.Muls)
            };

            return output;
        }
        private decimal[] TwoArraysInOne(decimal[] sums, int[] muls)
        {
            decimal[] decMuls = Array.ConvertAll(muls, x => (decimal)x);
            decimal[] unsortedArray = new decimal[sums.Length + decMuls.Length];

            sums.CopyTo(unsortedArray, 0);
            decMuls.CopyTo(unsortedArray, sums.Length);

            return unsortedArray.OrderBy(i => i).ToArray();
        }
    }
}
