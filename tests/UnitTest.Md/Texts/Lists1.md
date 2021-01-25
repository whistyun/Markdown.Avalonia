
First we have a simple numeric list

1. Alpha
1. Beta
2. Gamma
3. Delta

Then we have a simple unordered list

* Han
* Leia
* Luke
* Obiwan

## Unordered

Asterisks tight:

*	asterisk 1
*	asterisk 2
*	asterisk 3


Asterisks loose:

*	asterisk 1

*	asterisk 2

*	asterisk 3

* * *

Pluses tight:

+	Plus 1
+	Plus 2
+	Plus 3


Pluses loose:

+	Plus 1

+	Plus 2

+	Plus 3

* * *


Minuses tight:

-	Minus 1
-	Minus 2
-	Minus 3


Minuses loose:

-	Minus 1

-	Minus 2

-	Minus 3


## Ordered

Tight:

1.	First
2.	Second
3.	Third

and:

1. One
2. Two
3. Three


Loose using tabs:

1.	First

2.	Second

3.	Third

and using spaces:

1. One

2. Two

3. Three

Multiple paragraphs:

1.	Item 1, graf one.

	Item 2. graf two. The quick brown fox jumped over the lazy dog's
	back.
	
2.	Item 2.

3.	Item 3.



## Nested

*	Tab
	*	Tab
		*	Tab

Here's another:

1. First
2. Second:
	* Fee
	* Fie
	* Foe
3. Third

Same thing but with paragraphs:

1. First

2. Second:
	* Fee
	* Fie
	* Foe

3. Third


This was an error in Markdown 1.0.1:

*	this

	*	sub

	that


Mixing list

1. one
    1. one-one
    1. one-two
       inlist 1
        inlist 2
         inline 3
          inline 4

           as code
2. two
* three
* four
+ five
+ six
    + seven
+ eight
- eight
- nine

Enhanced syntax alphabet order.

ab. foo
a. bar
ab. foo2
a. bar2
a. foo3
a. bar3

Enhanced syntax roman order.

yes, we can.
no, i can.
i, one

===

ii, one

===

iii, one
yes, we can

===

iiii, one


* * *
* one
* * *
* one
 * * *
* one
  * * *
* two
```
code
```
* three
> quote
* four
# head
* four
## head

* * *
* one
   * * *
* two
 ```
 code
 ```
* three
 > quote
* four
 # head
* four
 ## head
