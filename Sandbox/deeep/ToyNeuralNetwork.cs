using System;
using System.Linq;
using System.Text.Json;
public class ToyNeuralNetwork : ICloneable
{
    private static Random _random = Random.Shared;
    private int[] _layers;//layers    
    private float[][] _neurons;//neurons    
    private float[][] _biases;//biasses    
    private float[][][] _weights;//weights    
    public ToyNeuralNetwork(int[] layers, float[][]? biases = null, float[][][]? weights = null)
    { 
        this._layers = new int[layers.Length];        
        for (int i = 0; i < layers.Length; i++)        
        {            
            this._layers[i] = layers[i];        
        }        
        this._neurons = InitNeurons();        
        this._biases = biases ?? InitBiases(true);        
        this._weights = weights ?? InitWeights(true);    
    }
    private float[][] InitNeurons()
    {        
        List<float[]> neuronsList = new List<float[]>();        
        for (int i = 0; i < _layers.Length; i++)        
        {            
            neuronsList.Add(new float[_layers[i]]);        
        }        
        return neuronsList.ToArray();    
    }
    private float[][] InitBiases(bool random = false)    
    {        
        List<float[]> biasList = new List<float[]>();        
        for (int i = 1; i < _layers.Length; i++)        
        {            
            float[] bias = new float[_layers[i]];            
            for (int j = 0; j < _layers[i]; j++)            
            {                
                bias[j] = random ? GetRandomFloatInRange(-0.5f, 0.5f) : 0f;            
            }            
            biasList.Add(bias);        
        }        
        return biasList.ToArray();    
    }
    private float[][][] InitWeights(bool random = false)   
    {        
        List<float[][]> weightsList = new List<float[][]>();        
        for (int i = 1; i < _layers.Length; i++)        
        {            
            List<float[]> layerWeightsList = new List<float[]>();   
            int neuronsInPreviousLayer = _layers[i - 1];            
            for (int j = 0; j < _neurons[i].Length; j++)            
            {                 
                float[] neuronWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)  
                {                                      
                    neuronWeights[k] = random ? GetRandomFloatInRange(-0.5f, 0.5f) : 0f;
                }               
                layerWeightsList.Add(neuronWeights);            
            }            
            weightsList.Add(layerWeightsList.ToArray());        
        }        
        return weightsList.ToArray();    
    }
    public float ActivateFunc(float value)    
    {        
        return float.Tanh(value);    
    }
    /// <summary>
    /// This function implements the feedforward operation of a neural network.
    /// It takes an array of input values and returns an array of output values.
    /// <para/>
    /// The function iterates over the layers of the neural network, computing the output
    /// of each layer based on the inputs from the previous layer and the weights and biases
    /// of the layer. The output of each layer is passed through an activation function to
    /// compute the final output of the network. The function returns the output values
    /// of the last layer of the network.
    /// </summary>
    public float[] FeedForward(float[] inputs)    
    {        
        for (int i = 0; i < inputs.Length; i++)        
        {            
            _neurons[0][i] = inputs[i];        
        }        
        for (int i = 1; i < _layers.Length; i++)        
        {            
            int layer = i - 1;            
            for (int j = 0; j < _neurons[i].Length; j++)            
            {                
                float value = 0f;               
                for (int k = 0; k < _neurons[i - 1].Length; k++)  
                {                    
                    value += _weights[i - 1][j][k] * _neurons[i - 1][k];      
                }                
                _neurons[i][j] = ActivateFunc(value + _biases[i - 1][j]);            
            }        
        }        
        return _neurons[_neurons.Length - 1];    
    }

    /// <summary>
    /// Computes the fitness of the neural network by evaluating its performance on a set of inputs and corresponding outputs.
    /// Inputs and outputs are represented as two-dimensional arrays, where each row corresponds to a single example and each column corresponds to a feature or output dimension.
    /// The fitness is computed as the inverse of the sum of absolute errors between the predicted outputs and the actual outputs for each example.
    /// Higher fitness values correspond to lower error and better performance.
    /// </summary>
    /// <param name="inputs">A two-dimensional array of input examples, where each row corresponds to a single example and each column corresponds to a feature dimension.</param>
    /// <param name="outputs">A two-dimensional array of output examples, where each row corresponds to a single example and each column corresponds to an output dimension.</param>
    /// <returns>The fitness of the neural network as a floating-point value.</returns>
    public float Test(float[][] inputs, float[][] outputs, bool print = false) {
        float sumError = 0;
        for (int i = 0; i < inputs.Length; i++) 
        {
            float[] output = FeedForward(inputs[i]);
            for (int j = 0; j < outputs[i].Length; j++) {
                sumError += Math.Abs(output[j] - outputs[i][j]);
                if (print) Console.WriteLine($"--> Expected {outputs[i][j]}, computed {output[j]}");
            }
        }
        if (print) Console.WriteLine($"==> Sum Error  {sumError}, fitness {1 / sumError}");
        return 1 / sumError; // higher fitness for lower error
    }

    public int Check(float[][] inputs, float[][] outputs, out int correct, out int failed)
    {
        correct = 0;
        failed = 0;
        for (int i = 0; i < inputs.Length; i++) 
        {
            float[] output = FeedForward(inputs[i]);
            for (int j = 0; j < outputs[i].Length; j++) {
                var decision = Math.Abs(output[j] - outputs[i][j]);
                if (decision < 0.5) 
                { 
                    correct +=1;
                }
                else
                {
                    failed +=1;
                }
            }
        }  
        return correct + failed;     
    }
 
    /// <summary>
    /// Mutates the weights and biases of the neural network with a specified probability and range of values.
    /// </summary>
    /// <param name="chance">The probability of each weight or bias being mutated.</param>
    /// <param name="val">The range of values that a weight or bias can be mutated by. The new value will be a random value within this range added to the original value.</param>
    public void Mutate(float chance, float val)
    {
        for (int i = 0; i < _biases.Length; i++)
        {
            for (int j = 0; j < _biases[i].Length; j++)
            {
                if (_random.NextSingle() <= chance) { _biases[i][j] += GetRandomFloatInRange(-val, val); }
            }
        }

        for (int i = 0; i < _weights.Length; i++)
        {
            for (int j = 0; j < _weights[i].Length; j++)
            {
                for (int k = 0; k < _weights[i][j].Length; k++)
                {
                    if  (_random.NextSingle() <= chance) { _weights[i][j][k] += GetRandomFloatInRange(-val, val); }
                }
            }
        }
    }

    float GetRandomFloatInRange(float minValue, float maxValue)
    {
        return (float)(_random.NextDouble() * (maxValue - minValue) + minValue);
    }

    public ToyNeuralNetwork CloneAndMutate(float chance, float val)
    {
        var nn = (this.Clone() as ToyNeuralNetwork);
        nn.Mutate(chance, val);
        return nn;
    }
    public object Clone() 
    {
        var nn = new ToyNeuralNetwork(this._layers);

        for (int i = 0; i < _biases.Length; i++)
        {
            for (int j = 0; j < _biases[i].Length; j++)
            {
                nn._biases[i][j] = _biases[i][j];
            }
        }
        for (int i = 0; i < _weights.Length; i++)
        {
            for (int j = 0; j < _weights[i].Length; j++)
            {
                for (int k = 0; k < _weights[i][j].Length; k++)
                {
                    nn._weights[i][j][k] = _weights[i][j][k];
                }
            }
        }
        return nn;
    }
    public void Save(string path)//this is used for saving the biases and weights within the network to a file.
    {
        
        var layersJson = JsonSerializer.Serialize(this._layers);
        var biasesJson = JsonSerializer.Serialize(this._biases);
        var weightsJson = JsonSerializer.Serialize(this._weights);
        
        File.WriteAllLines(path, new [] {layersJson, biasesJson, weightsJson});
    }
    public static ToyNeuralNetwork Load(string path)//this loads the biases and weights from within a file into the neural network.
    {
        var lines = File.ReadAllLines(path);
        var layersJson = lines[0];
        var biasesJson = lines[1];
        var weightsJson = lines[2];
        var layers = JsonSerializer.Deserialize<int[]>(layersJson);
        var biases = JsonSerializer.Deserialize<float[][]>(biasesJson);
        var weights = JsonSerializer.Deserialize<float[][][]>(weightsJson);

        var nn = new ToyNeuralNetwork(layers, biases, weights);
        return nn;
    }

    struct Model
    {
        public ToyNeuralNetwork Network {get; set;}
        public float Fitness {get; set;}
    }
    /// <summary>
    /// Trains the neural network using a genetic algorithm approach.
    /// </summary>
    /// <param name="inputs">A two-dimensional array of input examples, where each row corresponds to a single example and each column corresponds to a feature dimension.</param>
    /// <param name="outputs">A two-dimensional array of output examples, where each row corresponds to a single example and each column corresponds to an output dimension.</param>
    /// <param name="hiddenLayers">An array that represents the size of hidden network layers.</param>
    /// <param name="populationSize">The size of the population of candidate solutions.</param>
    /// <param name="mutationRate">The probability of a mutation occurring during breeding.</param>
    /// <param name="maxGenerations">The maximum number of generations to evolve the population.</param>
    /// <returns>The best-performing neural network obtained from the training process.</returns>
    public static ToyNeuralNetwork Train(float[][] inputs, float[][] outputs, int[] hiddenLayers, int populationSize, float mutationRate, float mutationValue, int maxGenerations) 
    {

        // represents the shape of the network...
        var networkLayers = new int [] {inputs[0].Length }.Concat(hiddenLayers).Append(outputs[0].Length).ToArray();
        Console.WriteLine($"Layers: {JsonSerializer.Serialize(networkLayers)}");

        Model GetNetworkFitness(ToyNeuralNetwork network)
        {
            var fit = network.Test(inputs, outputs);
            return new Model {Network = network, Fitness = fit};
        }
        Model[] GetNextGeneration(Model[] population)
        {
            Model[] nextGeneration = new Model[populationSize];
            for (int i = 0; i < populationSize; i++) 
            {
                // Select two parents using tournament selection
                ToyNeuralNetwork parent1 = TournamentSelect(population, populationSize/4).Network;
                ToyNeuralNetwork parent2 = TournamentSelect(population, populationSize/4).Network;

                // Create child by crossover
                ToyNeuralNetwork child = Crossover(parent1, parent2);

                // Mutate child with specified probability
                child.Mutate(mutationRate, mutationValue);

                // Add child to next generation
                nextGeneration[i] = GetNetworkFitness(child);
            }
            return nextGeneration;
        }

        // bootstrap with random networks:
        var population = Enumerable.Range(0, populationSize).Select(_ => GetNetworkFitness(new ToyNeuralNetwork(networkLayers))).OrderByDescending(x=>x.Fitness).ToArray();

        for (int generation = 0; generation < maxGenerations; generation++) 
        {
            var maxFitness = population.Max(x=>x.Fitness);
            var minFitness = population.Min(x=>x.Fitness);
            var avgFitness = population.Average(x=>x.Fitness);
            var medFitness = population[populationSize/2].Fitness;
            // Print best fitness for this generation
            Console.WriteLine($"Generation {generation}: Population = {population.Length}, Best fitness = {population[0].Fitness}, Avg fitness = {avgFitness}, Max={maxFitness}, Min={minFitness}, Med={medFitness}");
            // Check if we've found a perfect solution
            if (population[0].Fitness >= 1) {
                return population[0].Network; // return best-performing network
            }

            // Create next generation from the first half of best networks
            
            var firstQuarter = population.Take(populationSize/4);
            population = firstQuarter
                .Concat(firstQuarter.Select(n => GetNetworkFitness((n.Network.CloneAndMutate(mutationRate,mutationValue)))))
                .Concat(firstQuarter.Select(n => GetNetworkFitness((n.Network.CloneAndMutate(mutationRate,mutationValue)))))
                .Concat(firstQuarter.Select(n => GetNetworkFitness((n.Network.CloneAndMutate(mutationRate,mutationValue)))))
                .OrderByDescending(x=>x.Fitness).ToArray();
                
            //population = GetNextGeneration(population.Take(population.Length/4).ToArray()).OrderByDescending(x=>x.Fitness).ToArray();
        }
        return population[0].Network;
    }



    static Model TournamentSelect(Model[] population, int tournamentSize)
    {
        // Create a list to hold the tournament candidates
        var tournament = new List<Model>();

        // Add a random subset of the population to the tournament
        for (int i = 0; i < tournamentSize; i++)
        {
            int randomIndex = _random.Next(population.Length);
            tournament.Add(population[randomIndex]);
        }

        return tournament.MinBy(x=>x.Fitness);
    }



    public static ToyNeuralNetwork Crossover(ToyNeuralNetwork parent1, ToyNeuralNetwork parent2)
    {
        // Create a new neural network with the same structure as the parents
        var offspring = new ToyNeuralNetwork(parent1._layers);

        // Crossover the biases of the two parents
        for (int i = 0; i < parent1._biases.Length; i++)
        {
            for (int j = 0; j < parent1._biases[i].Length; j++)
            {
                if (_random.NextDouble() < 0.5)
                {
                    offspring._biases[i][j] = parent1._biases[i][j];
                }
                else
                {
                    offspring._biases[i][j] = parent2._biases[i][j];
                }
            }
        }

        // Crossover the weights of the two parents
        for (int i = 0; i < parent1._weights.Length; i++)
        {
            for (int j = 0; j < parent1._weights[i].Length; j++)
            {
                for (int k = 0; k < parent1._weights[i][j].Length; k++)
                {
                    if (_random.NextDouble() < 0.5)
                    {
                        offspring._weights[i][j][k] = parent1._weights[i][j][k];
                    }
                    else
                    {
                        offspring._weights[i][j][k] = parent2._weights[i][j][k];
                    }
                }
            }
        }

        return offspring;
    }

}