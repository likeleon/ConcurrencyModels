(defn recur-sum [coll result]
  (if (empty? coll) result
    (recur (rest coll) (+ (first coll) result))))