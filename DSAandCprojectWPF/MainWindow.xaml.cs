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
    using System.Collections.Generic;

    public partial class MainWindow : Window
    {
        private DoublyLinkedList<string> text = new DoublyLinkedList<string>();
        private Stack<string> textStack = new Stack<string>();
        private string allText = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Save the current text state before making changes
                textStack.Push(inputTextBox.Text);

                // Move the cursor to the end of the text
                inputTextBox.CaretIndex = inputTextBox.Text.Length;

                // Add a new line character
                inputTextBox.Text += Environment.NewLine;

                // Move the cursor to the end of the new line
                inputTextBox.CaretIndex = inputTextBox.Text.Length;

                // Scroll to the end of the text box
                inputTextBox.ScrollToEnd();

                // Prevent further processing by other event handlers
                e.Handled = true;
            }
        }

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

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {

            // Restore the previous text state
            inputTextBox.Text = textStack.Pop();

            // Set the caret index to the end of the restored text
            inputTextBox.CaretIndex = inputTextBox.Text.Length;
        }
    }

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
            if (top > size)
                throw new Exception("Stack Overflow");
            stack[top] = element;
            top++;
        }

        public T Pop()
        {
            if (IsEmpty())
                throw new Exception("Stack Underflow");
            else
            {
                top--;
                return stack[top];
            }
        }
    }

}