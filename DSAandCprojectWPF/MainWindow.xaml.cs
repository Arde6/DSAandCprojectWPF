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
    using System.Text.RegularExpressions;
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
            // Save the current text state before making changes
            textStack.Push(new TextRange(inputTextBox.Document.ContentStart, inputTextBox.Document.ContentEnd).Text);
        }


        // Updates InputTextBox
        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Remove the TextChanged event handler
            inputTextBox.TextChanged -= InputTextBox_TextChanged;

            string keywords = @"\b(auto|double|int|struct|break|else|long|switch|case|enum|register|typedef|char|extern|return|union|const|float|short|unsigned|continue|for|signed|void|default|goto|sizeof|volatile|do|if|static|while)\b";
            Regex keywordRegex = new Regex(keywords);

            TextRange textRange = new TextRange(inputTextBox.Document.ContentStart, inputTextBox.Document.ContentEnd);
            TextPointer currentPointer = textRange.Start.GetInsertionPosition(LogicalDirection.Forward);

            while (currentPointer != null)
            {
                TextPointer nextPointer = currentPointer.GetNextContextPosition(LogicalDirection.Forward);
                if (nextPointer != null)
                {
                    TextRange contextRange = new TextRange(currentPointer, nextPointer);
                    string[] lines = contextRange.Text.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.Contains("//"))
                        {
                            // Find the index where '//' starts
                            int index = line.IndexOf("//");

                            // Get the start position of '//' in the line
                            TextPointer commentStart = currentPointer.GetPositionAtOffset(index, LogicalDirection.Forward);

                            // Get the end position of the line
                            TextPointer commentEnd = currentPointer.GetPositionAtOffset(line.Length, LogicalDirection.Forward);

                            // Create a new text range from the start of '//' to the end of the line
                            TextRange commentRange = new TextRange(commentStart, commentEnd);

                            // Color the rest of the line
                            commentRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
                        }
                        else if (line.Contains("/*") && line.Contains("*/"))
                        {
                            // Color everything between /* and */
                            int start = line.IndexOf("/*");
                            int end = line.IndexOf("*/") + 2;
                            TextPointer commentStart = currentPointer.GetPositionAtOffset(start, LogicalDirection.Forward);
                            TextPointer commentEnd = commentStart.GetPositionAtOffset(end - start, LogicalDirection.Forward);
                            TextRange commentRange = new TextRange(commentStart, commentEnd);
                            commentRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Green);
                        }
                        else
                        {
                            string[] words = line.Split(' ');
                            foreach (string word in words)
                            {
                                if (keywordRegex.IsMatch(word))
                                {
                                    int index = line.IndexOf(word);
                                    TextPointer wordStart = currentPointer.GetPositionAtOffset(index, LogicalDirection.Forward);
                                    TextPointer wordEnd = wordStart.GetPositionAtOffset(word.Length, LogicalDirection.Forward);
                                    TextRange wordRange = new TextRange(wordStart, wordEnd);
                                    wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                                }
                                else
                                {
                                    int index = line.IndexOf(word);
                                    TextPointer wordStart = currentPointer.GetPositionAtOffset(index, LogicalDirection.Forward);
                                    TextPointer wordEnd = wordStart.GetPositionAtOffset(word.Length, LogicalDirection.Forward);
                                    TextRange wordRange = new TextRange(wordStart, wordEnd);
                                    wordRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                                }
                            }
                        }
                    }
                }
                currentPointer = nextPointer;
            }

            // Add the TextChanged event handler back
            inputTextBox.TextChanged += InputTextBox_TextChanged;
        }


        // Buttons for ui
        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {

                // Restore the previous text state
                string previousText = textStack.Pop();

                // Set the text content of the RichTextBox
                TextRange textRange = new TextRange(inputTextBox.Document.ContentStart, inputTextBox.Document.ContentEnd);
                textRange.Text = previousText;

                // Set the caret index to the end of the restored text
                inputTextBox.CaretPosition = inputTextBox.Document.ContentEnd;
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
            if (query == null || query.Length == 0 || inputTextBox == null) return;

            TextRange textRange = new TextRange(inputTextBox.Document.ContentStart, inputTextBox.Document.ContentEnd);
            string text = textRange.Text;

            Trie trie = BuildTrie(text);

            occurrences = FindAllOccurrences(text, query);
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
                TextPointer start = inputTextBox.Document.ContentStart.GetPositionAtOffset(index);
                TextPointer end = inputTextBox.Document.ContentStart.GetPositionAtOffset(index + query.Length);

                if (start != null && end != null)
                {
                    TextRange range = new TextRange(start, end);
                    range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);

                    // Calculate the line number based on the index
                    int lineIndex = GetLineIndexFromPosition(inputTextBox.Document.ContentStart, start);
                    double verticalOffset = 0;

                    if (lineIndex >= 0)
                    {
                        // Get the line corresponding to the index
                        var line = GetVisualChild<Paragraph>(inputTextBox, lineIndex);
                        // Calculate the vertical offset based on the line's position
                        verticalOffset = line.Margin.Top + line.Margin.Bottom;
                    }

                    // Scroll to the calculated vertical offset
                    inputTextBox.ScrollToVerticalOffset(verticalOffset);
                }
            }
        }

        private int GetLineIndexFromPosition(TextPointer start, TextPointer position)
        {
            int lineIndex = 0;
            while (start.CompareTo(position) < 0)
            {
                if (start.GetLineStartPosition(1) == null)
                    break;

                start = start.GetLineStartPosition(1);
                lineIndex++;
            }
            return lineIndex;
        }

        private T GetVisualChild<T>(DependencyObject parent, int index) where T : DependencyObject
        {
            int count = 0;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    if (count == index)
                        return typedChild;
                    count++;
                }
            }
            return null;
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

            public Stack(int size = 200)
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



        private void ApplySyntaxHighlighting()
        {
            // Get the text from the RichTextBox
            string text = new TextRange(inputTextBox.Document.ContentStart, inputTextBox.Document.ContentEnd).Text;

            // Define regex patterns for syntax highlighting
            Dictionary<string, SolidColorBrush> patterns = new Dictionary<string, SolidColorBrush>
            {
                { @"//.*$", Brushes.Green }, // Single-line comments
                { @"(/\*.*?\*/)|(/\*.*)", Brushes.Green }, // Multi-line comments
                { @"\b(class|void|int|string|public|private)\b", Brushes.Blue }, // Keywords
                { @"\b(true|false|null)\b", Brushes.DarkOrange }, // Constants
                { @"""[^""\\]*(?:\\.[^""\\]*)*""", Brushes.Red }, // Strings
                { @"'.*?'", Brushes.Red }, // Characters
                { @"\b[A-Za-z_]\w*\b", Brushes.Black } // Identifiers
            };

            // Apply formatting to matches
            foreach (var pattern in patterns)
            {
                Regex regex = new Regex(pattern.Key, RegexOptions.IgnoreCase);
                foreach (Match match in regex.Matches(text))
                {
                    ApplyFormatting(match.Index, match.Length, pattern.Value);
                }
            }
        }

        private void ApplyFormatting(int startIndex, int length, SolidColorBrush color)
        {
            TextPointer start = inputTextBox.Document.ContentStart.GetPositionAtOffset(startIndex);
            TextPointer end = inputTextBox.Document.ContentStart.GetPositionAtOffset(startIndex + length);

            if (start != null && end != null)
            {
                TextRange range = new TextRange(start, end);
                range.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            }
        }

    }
}