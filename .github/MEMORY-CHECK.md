# Checking whether a program leaks memory (no coding needed)

Use this when a program seems to slow down, or to fill the computer's or
virtual machine's memory, after running for a long time.

A memory leak doesn't show an error. The program just keeps using more memory
the longer it runs, until the machine slows down or runs out.

## 1. Watch the number

1. Open **Task Manager** (Ctrl+Shift+Esc) and go to the **Details** tab.
2. Find the program in the list.
3. Add the **"Memory (private working set)"** column: right-click any column
   header, choose **Select columns**, and tick it.
4. Write down the number and the time.
5. Leave the program running as usual, and write the number down again every
   hour or so.

Some growth at the start is normal. A number that keeps climbing hour after
hour, and never comes back down, is a leak.

## 2. Find out what makes it grow

- **Grows while nobody uses the program:** something inside it runs on its
  own, such as a timer, logging, or data being loaded again and again. This is
  most likely in the program's own code, not in the ribbon.
- **Grows each time you do something:** for example opening and closing
  windows, right-clicking the ribbon, or adding and removing Quick Access
  Toolbar buttons. Repeat that action 20 times and check whether the number
  rises by about the same amount each round. If it does, that action leaks.
  The ribbon leaks fixed on 2026-10-09 (`architecture/MEMORY-LEAKS.md`) were
  of this kind.

## 3. Compare before and after a fix

Run the same steps with the old version and the new one: same actions, same
amount of time. Note both sets of numbers. If the new version stays flat where
the old one climbed, the fix works in practice.

## 4. If you need to know exactly what fills the memory

Task Manager can save a snapshot of the program's memory: right-click the
program, then choose **Create memory dump file**. Claude can examine a snapshot
and say what is filling the memory.

**Be careful before sharing one.** A snapshot contains everything the program
had in memory, which can include passwords, customer data or other private
information. It is also often hundreds of megabytes. Only share it if the
program doesn't handle anything sensitive, and never attach it to a public
issue.
