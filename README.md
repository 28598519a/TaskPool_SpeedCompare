# TaskPool_SpeedCompare
用於評估 限制Task並發數量 時不同方法的速度

Used to evaluate the speed of different method when limiting the number of concurrent tasks
 
根據目前的測試結果第2種方法是最好的<br>
雖然網路上很多文章都使用SemaphoreSlim的方法，但根據測試結果他非常緩慢，請不要使用<br>
請不要使用第三種方法(Parallel.ForEach)，因為根據測試結果有可能Task不會被全部執行

According to the test results so far, the second method is the best.<br>
Although many articles on the web use SemaphoreSlim's method, it is very slow according to test results, please do not use it.<br>
Do not use the third method (parallel.foreach) because it is possible that the Task will not be fully executed based on the test result.

![Result](https://user-images.githubusercontent.com/33422418/136525185-c605011d-5755-4f6b-a899-0d9a5d02f7c0.png)
