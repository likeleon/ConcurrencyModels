(defn recur-sum [coll result]
  (if (empty? coll) result
    (recur (rest coll) (+ (first coll) result))))

(defn reduce-sum [coll]
  (reduce (fn [acc x] (+ acc x)) 0 coll))

(defn reduce-sum2 [coll]
  (reduce #(+ % %2) 0 coll))