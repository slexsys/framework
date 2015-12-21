﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Accord.MachineLearning.DecisionTrees;
using Accord.Statistics.Filters;
using Accord.MachineLearning.DecisionTrees.Learning;
using Accord.Math;


namespace Accord.MachineLearning.DecisionTrees
{
	// wrapper class for a decision tree with random forest hyperparameters
    [Serializable]
    class ForestTree
    {
		// categorical value encoding table
        private Codification Codebook = null;
		// proportion of features to use at each split
        private double PcntFeaturesToUse = 0;
		// names of feature columns
        private string[] InputCols = null;
		// name of label column
        private string OutputCol = null;
		// decision tree
        private DecisionTree Tree = null;
        List<DecisionVariable> Attributes = null;

        public ForestTree(double pcntFeaturesToUse, string[] inputCols, string outputCol, Codification codebook, List<DecisionVariable> attributes)
        {
            PcntFeaturesToUse = pcntFeaturesToUse;
            InputCols = inputCols;
            OutputCol = outputCol;
            Codebook = codebook;
            Attributes = attributes;
        }

        public void Fit(DataTable symbols)
        {
            double[][] inputs = symbols.ToArray(InputCols);
            int[] outputs = symbols.ToArray<int>(OutputCol);
            Tree = new DecisionTree(Attributes, 2);
            Tree.pcntAttributesToUse = PcntFeaturesToUse;
            C45Learning c45 = new C45Learning(Tree);
            c45.Join = 100;
            c45.Run(inputs, outputs);
        }

        /// <summary>
        ///   Loads a tree from a file.
        /// </summary>
        /// 
        /// <param name="path">The path to the file from which the tree is to be deserialized.</param>
        /// 
        /// <returns>The deserialized tree.</returns>
        /// 
        public void Save(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                Save(fs);
            }
        }

        /// <summary>
        ///   Saves the tree to a stream.
        /// </summary>
        /// 
        /// <param name="stream">The stream to which the tree is to be serialized.</param>
        /// 
        public void Save(Stream stream)
        {
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(stream, this);
        }

        /// <summary>
        ///   Loads a tree from a stream.
        /// </summary>
        /// 
        /// <param name="stream">The stream from which the tree is to be deserialized.</param>
        /// 
        /// <returns>The deserialized tree.</returns>
        /// 
        public static ForestTree Load(Stream stream)
        {
            BinaryFormatter b = new BinaryFormatter();
            return (ForestTree)b.Deserialize(stream);
        }

        /// <summary>
        ///   Loads a tree from a file.
        /// </summary>
        /// 
        /// <param name="path">The path to the tree from which the machine is to be deserialized.</param>
        /// 
        /// <returns>The deserialized tree.</returns>
        /// 
        public static ForestTree Load(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                return Load(fs);
            }
        }

        public int[] Predict(DataTable symbols)
        {
            double[][] inputs = symbols.ToArray();
            return inputs.Select(x => predict(x)).ToArray();
        }

        private int predict(double[] data)
        {
            return Tree.Compute(data);
        }

    }
}
