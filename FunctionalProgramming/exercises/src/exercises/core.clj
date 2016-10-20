(ns exercises.core)

; day 1
(defn recursive-sum-recur [numbers]
  (loop [acc 0
         remaining-numbers numbers]
    (if (empty? remaining-numbers)
      acc
      (recur (+ acc (first remaining-numbers)) (rest remaining-numbers)))))

(defn reduce-sum [numbers]
  (reduce #(+ %1 %2) 0 numbers))