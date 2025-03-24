module Exercise03_02 =

    type BillingDetails = {
        name : string
        billing :  string
        delivery : string option }

    let myOrder = {
        name = "Kit Eason"
        billing = "112 Fibonacci Street\nErehwon\n35813"
        delivery = None }

    let hisOrder = {
        name = "John Doe"
        billing = "314 Pi Avenue\nErewhon\n15926"
        delivery = None }

    let herOrder = {
        name = "Jane Smith"
        billing = null
        delivery = None }

    let orders = [| myOrder; hisOrder; herOrder |]

    // let countNullBillingAddresses (orders : seq<BillingDetails>) = 
    //     orders
    //     |> ..
    
// What is the most concise function you can write to count the number of BillingDetails
// instances that have a nonnull billing address? (Ignore the delivery address.)

// Hint: One way to solve this is using two functions from the Option module. Option.ofObj
// is one of them. You might also want to use Seq.map and Seq.sumBy.