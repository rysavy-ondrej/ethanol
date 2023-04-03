// See https://aka.ms/new-console-template for more information
Console.WriteLine("Toy Neural Network:");

// we read data from CSV file and train the neural network:

var records = GenderRecord.Load(@"data/gender.csv").ToArray();
var arrays = records.Select(x=> x.AsFloatArray()).Unzip();
var input = arrays.Item2.ToArray();
var output = arrays.Item1.ToArray();

for(int i = 0; i <  input.Length; i++)
{
    Console.WriteLine($"[{String.Join(';',input[i])}]->[{String.Join(';', output[i])}] -- {records[i]}");
}


var model = ToyNeuralNetwork.Train(input, output, new int[] { 8, 6 }, 128, 0.4f, 0.25f, 5000);

model.Check(input, output, out var correct, out var wrong);

Console.WriteLine($"Result: TP = {correct}, FP = {wrong}");

model.Save("gender.model");
