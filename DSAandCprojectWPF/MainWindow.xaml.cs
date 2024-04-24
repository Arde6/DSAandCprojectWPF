using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DSAandCprojectWPF
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml.Serialization;

    public partial class MainWindow : Window
    {
        private DoublyLinkedList<string> text = new DoublyLinkedList<string>();
        private Stack<string> textStack = new Stack<string>();
        private string allText = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        // KeyDown for InputTextBox
        // space and enter for making undo work
        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int caretIndex = inputTextBox.CaretIndex;

            if (e.Key == Key.Enter)
            {
                // Save the current text state before making changes
                textStack.Push(inputTextBox.Text);

                // Insert a new line character at the current caret index
                inputTextBox.Text = inputTextBox.Text.Insert(caretIndex, Environment.NewLine);

                // Move the caret to the next line
                inputTextBox.CaretIndex = caretIndex + Environment.NewLine.Length;

                // Scroll to the caret position
                inputTextBox.ScrollToVerticalOffset(inputTextBox.VerticalOffset + inputTextBox.FontSize);

                // Prevent further processing by other event handlers
                e.Handled = true;
            }
            if (e.Key == Key.Space)
            {
                // Save the current text state before making changes
                textStack.Push(inputTextBox.Text);

                // Move the cursor to the end of the text
                inputTextBox.CaretIndex = inputTextBox.Text.Length;

                // Add a space character to the text content
                inputTextBox.Text += " ";

                // Explicitly set the Text property of the TextBox
                inputTextBox.Text = inputTextBox.Text;

                // Restore the caret index
                inputTextBox.CaretIndex = caretIndex + 1;

                // Prevent further processing by other event handlers
                e.Handled = true;
            }
        }



        // Updates InputTextBox
        private void UpdateDisplay()
        {
            StringBuilder sb = new StringBuilder();
            DoublyLinkedListNode<string> current = text.head;
            while (current != null)
            {
                sb.Append(current.Data);
                current = current.Next;
            }
            inputTextBox.Text = sb.ToString();
        }


        // Buttons for ui
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {

            // Restore the previous text state
            inputTextBox.Text = textStack.Pop();

            // Set the caret index to the end of the restored text
            inputTextBox.CaretIndex = inputTextBox.Text.Length;
        }

        private void SerachClick(object sender, RoutedEventArgs e)
        {
            editorFind(serachInput.Text);
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void dragWindow(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (occurrences.Count > 0)
            {
                currentOccurrenceIndex = (currentOccurrenceIndex + 1) % occurrences.Count;
                HighlightCurrentOccurrence(serachInput.Text);
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (occurrences.Count > 0)
            {
                currentOccurrenceIndex = (currentOccurrenceIndex - 1 + occurrences.Count) % occurrences.Count;
                HighlightCurrentOccurrence(serachInput.Text);
            }
        }


        // Search function
        private List<int> occurrences;
        private int currentOccurrenceIndex = -1;

        private void editorFind(string query)
        {
            if (query == null || query.Length == 0) return;

            Trie trie = BuildTrie(inputTextBox.Text);

            occurrences = FindAllOccurrences(inputTextBox.Text, query);
            if (occurrences.Count > 0)
            {
                currentOccurrenceIndex = 0;
                HighlightCurrentOccurrence(query);
            }
            else
            {
                // Show a message box indicating that the query was not found
                MessageBox.Show($"Could not find {query}", "Search Result", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void HighlightCurrentOccurrence(string query)
        {
            if (currentOccurrenceIndex >= 0 && currentOccurrenceIndex < occurrences.Count)
            {
                int index = occurrences[currentOccurrenceIndex];
                inputTextBox.Select(index, query.Length);
                // Highlight the found text by changing the background color
                inputTextBox.SelectionBrush = Brushes.Yellow;
                inputTextBox.Focus();
                // Scroll to the occurrence
                inputTextBox.ScrollToLine(inputTextBox.GetLineIndexFromCharacterIndex(index));
            }
        }

        private List<int> FindAllOccurrences(string text, string query)
        {
            List<int> occurrences = new List<int>();
            int index = 0;
            while ((index = text.IndexOf(query, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                occurrences.Add(index);
                index += query.Length;
            }
            return occurrences;
        }

        private Trie BuildTrie(string text)
        {
            Trie trie = new Trie();
            string[] words = text.Split(new char[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words)
            {
                trie.Insert(word);
            }
            return trie;
        }

        public class TrieNode
        {
            public Dictionary<char, TrieNode> Children { get; } = new Dictionary<char, TrieNode>();
            public bool IsEndOfWord { get; set; }
        }

        public class Trie
        {
            private readonly TrieNode root = new TrieNode();

            public void Insert(string word)
            {
                TrieNode current = root;
                foreach (char c in word)
                {
                    if (!current.Children.ContainsKey(c))
                    {
                        current.Children[c] = new TrieNode();
                    }
                    current = current.Children[c];
                }
                current.IsEndOfWord = true;
            }

            public bool Search(string word)
            {
                TrieNode node = SearchNode(word);
                return node != null && node.IsEndOfWord;
            }

            private TrieNode SearchNode(string word)
            {
                TrieNode current = root;
                foreach (char c in word)
                {
                    if (current.Children.ContainsKey(c))
                    {
                        current = current.Children[c];
                    }
                    else
                    {
                        return null;
                    }
                }
                return current;
            }
        }


        // Class for text
        public class DoublyLinkedListNode<T>
        {
            public T Data { get; set; }
            public DoublyLinkedListNode<T> Previous { get; set; }
            public DoublyLinkedListNode<T> Next { get; set; }

            public DoublyLinkedListNode(T data)
            {
                Data = data;
                Previous = null;
                Next = null;
            }
        }

        public class DoublyLinkedList<T>
        {
            public DoublyLinkedListNode<T> head;
            public DoublyLinkedListNode<T> tail;

            public DoublyLinkedList()
            {
                head = null;
                tail = null;
            }

            public void InsertAtEnd(T data)
            {
                DoublyLinkedListNode<T> newNode = new DoublyLinkedListNode<T>(data);
                if (head == null)
                {
                    head = newNode;
                    tail = newNode;
                }
                else
                {
                    tail.Next = newNode;
                    newNode.Previous = tail;
                    tail = newNode;
                }
            }

            public void DeleteAtEnd()
            {
                if (tail == null)
                {
                    Console.WriteLine("List is empty");
                    return;
                }
                if (head == tail)
                {
                    head = null;
                    tail = null;
                    return;
                }
                tail = tail.Previous;
                tail.Next = null;
            }

            public void Display()
            {
                DoublyLinkedListNode<T> current = head;
                while (current != null)
                {
                    string dataAsString = current.Data as string;
                    if (dataAsString != null && dataAsString.Length > 0 && dataAsString[dataAsString.Length - 1] != '\n')
                    {
                        Console.Write(dataAsString + " ");
                    }
                    current = current.Next;
                }
                Console.WriteLine();
            }

        }


        // Class for undo
        public class Stack<T>
        {
            private int top = 0;
            private int size;

            private T[] stack;

            public Stack(int size = 10)
            {
                this.size = size;
                stack = new T[size];
            }

            public bool IsEmpty()
            {
                if (top == 0)
                    return true;
                else
                    return false;
            }

            public void Push(T element)
            {
                if (top >= size)
                    this.PopFirst();
                stack[top] = element;
                top++;
            }

            public T Pop()
            {
                if (IsEmpty())
                    return stack[top];
                else
                {
                    top--;
                    return stack[top];
                }
            }
            public T PopFirst()
            {
                if (IsEmpty())
                    throw new Exception("Stack Underflow");

                T poppedElement = stack[0];
                // Shift elements to the left
                for (int i = 0; i < top - 1; i++)
                {
                    stack[i] = stack[i + 1];
                }
                top--;
                stack[top] = default(T); // Reset the value to default
                return stack[top];
            }

        }

    }
}