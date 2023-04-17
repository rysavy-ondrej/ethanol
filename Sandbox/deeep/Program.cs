// See https://aka.ms/new-console-template for more information


void MalwareModelTest()
{
     Console.WriteLine("Malware Model Classification");

    // we read data from CSV file and train the neural network:
    var records = FlowRecord.LoadCsv(@"data/malware.csv").ToList();

    var protocolEncoder = new OneHotEncoder(records.Select(x=>x.Protocol));
    var labelEncoder = new OneHotEncoder(records.Select(x=>x.Label));
    var familyEncoder = new OneHotEncoder(records.Select(x=>x.Family));

    var arrays = records.Select(x=> x.AsFloatArray(protocolEncoder, labelEncoder, familyEncoder)).Unzip(x=>x.Input, x=>x.Output);
    var input = arrays.Item1.ToArray();
    var output = arrays.Item2.ToArray();

    for(int i = 0; i <  input.Length; i++)
    {
        Console.WriteLine($"[{String.Join(';',input[i])}]->[{String.Join(';', output[i])}] -- {records[i]}");
    }


    var model = ToyNeuralNetwork.Train(input, output, new int[] { 8 }, 32, 0.5f, 0.25f, 5000);

    model.Check(input, output, out var correct, out var wrong);

    Console.WriteLine($"Result: TP = {correct}, FP = {wrong}");

    model.Save("malware.model");   
}

void GenderModelTest()
{
    Console.WriteLine("Gender Model Classification");

    // we read data from CSV file and train the neural network:
    var records = GenderRecord.LoadCsv(@"data/gender.csv").ToArray();
    var arrays = records.Select(x=> x.AsFloatArray()).Unzip(x=>x.Input, x=>x.Output);
    var input = arrays.Item1.ToArray();
    var output = arrays.Item2.ToArray();

    for(int i = 0; i <  input.Length; i++)
    {
        Console.WriteLine($"[{String.Join(';',input[i])}]->[{String.Join(';', output[i])}] -- {records[i]}");
    }


    var model = ToyNeuralNetwork.Train(input, output, new int[] { 8, 6 }, 128, 0.4f, 0.25f, 5000);

    model.Check(input, output, out var correct, out var wrong);

    Console.WriteLine($"Result: TP = {correct}, FP = {wrong}");

    model.Save("gender.model");
}

MalwareModelTest();